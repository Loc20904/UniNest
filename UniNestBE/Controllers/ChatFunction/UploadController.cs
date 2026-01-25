using Microsoft.AspNetCore.Mvc;

namespace UniNestBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("chat-image")]
        public async Task<IActionResult> UploadChatImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn ảnh.");

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            // --- SỬA LỖI TẠI ĐÂY ---
            // Kiểm tra nếu WebRootPath null thì tự trỏ vào thư mục wwwroot trong Project
            string webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var uploadFolder = Path.Combine(webRootPath, "uploads", "chat");
            // -----------------------

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về URL
            var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/chat/{fileName}";

            return Ok(new { url = fileUrl });
        }
    }
}