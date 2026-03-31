using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // 1. Tìm user theo Email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                // 2. Kiểm tra User tồn tại và verify Password
                if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác." });
                }

                // Kiểm tra trạng thái xác thực đối với sinh viên
                if (user.Role == "student" && !user.IsVerified)
                {
                    return Unauthorized(new { message = "Tài khoản của bạn đang chờ quản trị viên xét duyệt thẻ sinh viên. Vui lòng thử lại sau." });
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
            catch (Exception ex)
            {
                // Trả về Unauthorized với thông báo lỗi thay vì 500 nếu hash không hợp lệ
                return Unauthorized(new { message = "Lỗi xác thực: Mật khẩu không hợp lệ hoặc dữ liệu người dùng bị lỗi." });
            }
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Với JWT stateless, Server không cần làm gì nhiều.
            // Client chịu trách nhiệm xóa token khỏi localStorage/Cookies.
            return Ok(new { message = "Đăng xuất thành công. Vui lòng xóa token ở phía Client." });
        }

        // POST: api/Auth/request-registration
        [AllowAnonymous]
        [HttpPost("request-registration")]
        public async Task<IActionResult> RequestRegistration([FromBody] RequestRegistrationDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Email))
                    return BadRequest(new { message = "Email không được để trống." });

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email này đã được sử dụng." });
                }

                // Generate a temporary JWT token just for registration containing the email
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, dto.Email),
                    new Claim("IsRegistrationToken", "true")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1), // Link valid for 1 hour
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Send Email
                var registerLink = $"https://localhost:5015/register-complete?token={tokenString}";
                var allowedDomains = await _context.AllowedEmailDomains.Select(d => d.DomainName).ToListAsync();
                bool isEduEmail = allowedDomains.Any(d => dto.Email.EndsWith(d, StringComparison.OrdinalIgnoreCase));
                var emailType = isEduEmail ? "Tài khoản sinh viên" : "Tài khoản (cần xét duyệt)";
                var emailBody = $@"
                    <h3>Hoàn tất đăng ký UniNest</h3>
                    <p>Chào bạn,</p>
                    <p>Bạn đã yêu cầu đăng ký <b>{emailType}</b> trên UniNest.</p>
                    <p>Vui lòng click vào link bên dưới để tiếp tục thiết lập tài khoản:</p>
                    <p><a href='{registerLink}' style='display:inline-block; padding: 10px 20px; background-color: #00bda4; color: white; text-decoration: none; border-radius: 5px;'>Tiếp tục Đăng ký</a></p>
                    <p>Link này sẽ hết hạn sau 1 giờ.</p>";

                await _emailService.SendEmailAsync(dto.Email, "Xác nhận Đăng ký UniNest", emailBody);

                return Ok(new { message = "Link hoàn tất đăng ký đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        // POST: api/Auth/complete-registration
        [AllowAnonymous]
        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromForm] CompleteRegistrationDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Token)) return BadRequest(new { message = "Lỗi Binding: dto.Token is null. Frontend gửi form data không thành công." });

                // Decode and Validate Token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

                try
                {
                    tokenHandler.ValidateToken(dto.Token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var email = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == "email")?.Value;
                    var isRegToken = jwtToken.Claims.FirstOrDefault(x => x.Type == "IsRegistrationToken")?.Value;

                    if (string.IsNullOrEmpty(email))
                        return BadRequest(new { message = "Token thiếu Claim Email." });

                    if (isRegToken != "true")
                        return BadRequest(new { message = "Token không mang Claim IsRegistrationToken=true." });

                    // Proceed with registration
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (existingUser != null)
                        return BadRequest(new { message = "Email này đã được sử dụng." });

                    var allowedDomains = await _context.AllowedEmailDomains.Select(d => d.DomainName).ToListAsync();
                    bool isEduEmail = allowedDomains.Any(d => email.EndsWith(d, StringComparison.OrdinalIgnoreCase));

                    // Validations for regular email
                    if (!isEduEmail)
                    {
                        if (dto.frontIdImage == null || dto.backIdImage == null || string.IsNullOrEmpty(dto.Semester))
                            return BadRequest(new { message = "Tài khoản đăng ký bằng email thường yêu cầu tải lên ảnh 2 mặt thẻ sinh viên và khai báo học kỳ." });
                    }

                    string passwordHash = string.IsNullOrEmpty(dto.Password) ? string.Empty : BCrypt.Net.BCrypt.HashPassword(dto.Password);

                    var newUser = new User
                    {
                        FullName = dto.FullName,
                        Email = email,
                        PhoneNumber = dto.PhoneNumber,
                        PasswordHash = passwordHash,
                        Role = string.IsNullOrEmpty(dto.Role) ? "student" : dto.Role,
                        IsVerified = isEduEmail, // Auto verify if edu email
                        IsOnline = false,
                        LastActiveAt = DateTime.UtcNow
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync(); // Save to get UserId

                    // If not edu email, handle files and VerificationRequest
                    if (!isEduEmail)
                    {
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "verifications");
                        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                        var frontFileName = $"{Guid.NewGuid()}_{dto.frontIdImage!.FileName}";
                        var frontFilePath = Path.Combine(uploadPath, frontFileName);
                        using (var stream = new FileStream(frontFilePath, FileMode.Create)) { await dto.frontIdImage.CopyToAsync(stream); }

                        var backFileName = $"{Guid.NewGuid()}_{dto.backIdImage!.FileName}";
                        var backFilePath = Path.Combine(uploadPath, backFileName);
                        using (var stream = new FileStream(backFilePath, FileMode.Create)) { await dto.backIdImage.CopyToAsync(stream); }

                        var verificationReq = new StudentVerificationRequest
                        {
                            UserId = newUser.UserId,
                            FrontIdImageUrl = $"/images/verifications/{frontFileName}",
                            BackIdImageUrl = $"/images/verifications/{backFileName}",
                            CurrentSemester = dto.Semester ?? "",
                            Status = "Pending",
                            SubmittedAt = DateTime.UtcNow
                        };

                        _context.StudentVerificationRequests.Add(verificationReq);
                        await _context.SaveChangesAsync();
                    }

                    return Ok(new { message = "Đăng ký tài khoản thành công." });
                }
                catch (SecurityTokenExpiredException)
                {
                    return BadRequest(new { message = "Link đăng ký đã hết hạn. Vui lòng yêu cầu link mới." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = $"Đã xảy ra lỗi trong quá trình xử lý: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Đã xảy ra lỗi hệ thống: {ex.Message}" });
            }
        }

        // POST: api/Auth/forgot-password
        [AllowAnonymous]
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
        [AllowAnonymous]
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