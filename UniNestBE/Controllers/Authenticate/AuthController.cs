using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UniNestBE.DTOs;
using UniNestBE.Services;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UniNestDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(UniNestDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Tìm user theo Email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // 2. Kiểm tra User tồn tại và verify Password
            // Lưu ý: Password trong DB nên được hash bằng BCrypt.
            // Nếu bạn đang lưu plain-text (không nên), hãy dùng: if (user.Password != loginDto.Password)
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác." });
            }

            // 3. Tạo Token
            var token = GenerateJwtToken(user);

            // 4. Trả về kết quả
            return Ok(new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                FullName = user.FullName,
                Role = user.Role
            });
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Với JWT stateless, Server không cần làm gì nhiều.
            // Client chịu trách nhiệm xóa token khỏi localStorage/Cookies.
            return Ok(new { message = "Đăng xuất thành công. Vui lòng xóa token ở phía Client." });
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                // 1. Kiểm tra Email đã tồn tại chưa
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email này đã được sử dụng." });
                }

                // 2. Mã hóa mật khẩu
                string passwordHash = string.IsNullOrEmpty(registerDto.Password) 
                    ? string.Empty 
                    : BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                // 3. Tạo User mới
                var newUser = new User
                {
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    Role = string.IsNullOrEmpty(registerDto.Role) ? "student" : registerDto.Role,
                    IsVerified = false,
                    IsOnline = false,
                    LastActiveAt = DateTime.UtcNow
                };

                // 4. Lưu vào cơ sở dữ liệu
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đăng ký tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotRequest)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotRequest.Email);
                if (user == null)
                {
                    // Trả về BadRequest để khớp với Failed message trên UI
                    return BadRequest(new { message = "Không tìm thấy tài khoản với email này." });
                }

                // 2. Sinh Token và lưu vào DB
                string token = Guid.NewGuid().ToString();
                user.ResetPasswordToken = token;
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                // 3. Gửi Email
                var resetLink = $"https://localhost:5015/reset-password?email={user.Email}&token={token}";
                var emailBody = $@"
                    <h3>Đặt lại mật khẩu UniNest</h3>
                    <p>Chào bạn,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản UniNest. Vui lòng click vào link bên dưới để đặt mật khẩu mới.</p>
                    <p><a href='{resetLink}' style='display:inline-block; padding: 10px 20px; background-color: #00bda4; color: white; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a></p>
                    <p>Link này sẽ hết hạn sau 15 phút. Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

                await _emailService.SendEmailAsync(user.Email, "Thiết lập lại mật khẩu UniNest", emailBody);

                return Ok(new { message = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetRequest)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetRequest.Email);
                if (user == null || user.ResetPasswordToken != resetRequest.Token || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Yêu cầu đổi mật khẩu không hợp lệ hoặc đã hết hạn." });
                }

                // Đặt mật khẩu mới
                user.PasswordHash = string.IsNullOrEmpty(resetRequest.NewPassword)
                    ? string.Empty
                    : BCrypt.Net.BCrypt.HashPassword(resetRequest.NewPassword);

                // Xóa token
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập ngay bây giờ." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi hệ thống: {ex.Message}" });
            }
        }

        // --- Hàm tạo Token riêng ---
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                // Quan trọng: Claim này dùng để lấy UserId sau này (User.FindFirst...)
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Student"), // Ví dụ role
                new Claim("FullName", user.FullName) // Custom claim
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token sống 7 ngày
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}