using Microsoft.AspNetCore.Mvc;
using UniNestBE.Services;

namespace UniNestBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentModerationController : ControllerBase
    {
        private readonly IAiModerationService _moderationService;

        public ContentModerationController(IAiModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        /// <summary>
        /// Scan a chat message for harmful content using AI (Gemini).
        /// </summary>
        [HttpPost("scan")]
        public async Task<IActionResult> ScanMessage([FromBody] ScanMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Ok(new ModerationResult { IsSafe = true });
            }

            var result = await _moderationService.ScanMessageAsync(request.Content);
            return Ok(result);
        }

        /// <summary>
        /// Scan multiple messages in batch (for loading chat history).
        /// </summary>
        [HttpPost("scan-batch")]
        public async Task<IActionResult> ScanBatch([FromBody] ScanBatchRequest request)
        {
            if (request.Messages == null || request.Messages.Count == 0)
            {
                return Ok(new List<BatchScanResult>());
            }

            var results = new List<BatchScanResult>();

            foreach (var msg in request.Messages)
            {
                var result = await _moderationService.ScanMessageAsync(msg.Content);
                results.Add(new BatchScanResult
                {
                    MessageId = msg.MessageId,
                    Result = result
                });
            }

            return Ok(results);
        }
    }

    // === Request/Response DTOs ===

    public class ScanMessageRequest
    {
        public string Content { get; set; } = "";
    }

    public class ScanBatchRequest
    {
        public List<BatchMessageItem> Messages { get; set; } = new();
    }

    public class BatchMessageItem
    {
        public int MessageId { get; set; }
        public string Content { get; set; } = "";
    }

    public class BatchScanResult
    {
        public int MessageId { get; set; }
        public ModerationResult Result { get; set; } = new();
    }
}
