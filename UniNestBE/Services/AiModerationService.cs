using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace UniNestBE.Services
{
    public class ModerationResult
    {
        public bool IsSafe { get; set; } = true;
        public string AlertType { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Advice { get; set; } = "";
        public string Severity { get; set; } = "Low"; // High, Medium, Low
        public string Category { get; set; } = "Safe"; // Scam, PersonalInfo, Phishing, Harassment, Safe
        public List<string> MatchedKeywords { get; set; } = new();
    }

    public interface IAiModerationService
    {
        Task<ModerationResult> ScanMessageAsync(string content);
    }

    public class GroqModerationService : IAiModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelId;
        private readonly ILogger<GroqModerationService> _logger;

        // Simple in-memory cache to avoid duplicate API calls
        private readonly Dictionary<string, (ModerationResult Result, DateTime CachedAt)> _cache = new();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public GroqModerationService(IConfiguration configuration, ILogger<GroqModerationService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["GroqApi:ApiKey"] ?? "";
            _modelId = configuration["GroqApi:ModelId"] ?? "llama-3.1-8b-instant";
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<ModerationResult> ScanMessageAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new ModerationResult { IsSafe = true };

            // Check cache first
            var cacheKey = content.ToLowerInvariant().Trim();
            if (_cache.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow - cached.CachedAt < _cacheDuration)
            {
                return cached.Result;
            }

            // Clean old cache entries
            var expiredKeys = _cache.Where(kvp => DateTime.UtcNow - kvp.Value.CachedAt > _cacheDuration)
                                    .Select(kvp => kvp.Key).ToList();
            foreach (var key in expiredKeys) _cache.Remove(key);

            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GROQ_API_KEY_HERE")
            {
                _logger.LogWarning("Groq API key is not configured. Skipping AI moderation.");
                return new ModerationResult { IsSafe = true };
            }

            try
            {
                var prompt = BuildPrompt(content);
                var url = "https://api.groq.com/openai/v1/chat/completions";

                var requestBody = new
                {
                    model = _modelId,
                    response_format = new { type = "json_object" },
                    messages = new[]
                    {
                        new { role = "system", content = prompt },
                        new { role = "user", content = $"Tin nhắn cần phân tích: \"{content}\"" }
                    },
                    temperature = 0.1,
                    max_tokens = 500
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Groq API error {StatusCode}: {Body}", response.StatusCode, errorBody);
                    throw new HttpRequestException($"Groq API Error: {response.StatusCode}"); 
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Groq API Response JSON for '{Content}':\n{Response}", content, responseJson);
                var result = ParseGroqResponse(responseJson);

                // Cache the result
                _cache[cacheKey] = (result, DateTime.UtcNow);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Groq API for content moderation");
                return new ModerationResult { IsSafe = true }; // Fail-open
            }
        }

        private string BuildPrompt(string content)
        {
            return @"Bạn là hệ thống kiểm duyệt nội dung chat cho nền tảng tìm phòng trọ sinh viên.

Phân tích tin nhắn người dùng cung cấp và phân loại theo các category:
- **Scam**: Lừa đảo cọc phòng, hối thúc chuyển tiền, yêu cầu đặt cọc gấp, hứa hẹn giá rẻ bất thường
- **PersonalInfo**: Yêu cầu gửi CCCD/CMND, mật khẩu, OTP, thông tin ngân hàng qua chat
- **Phishing**: Link rút gọn đáng ngờ, mời vào nhóm lạ, link đăng nhập giả
- **Harassment**: Ngôn từ thô tục, xúc phạm, quấy rối, đe dọa
- **Safe**: Tin nhắn bình thường, hỏi thăm phòng, hỏi giá, trao đổi thông tin hợp lý

Lưu ý:
- Bất kỳ tin nhắn nào nhắc đến việc ""cọc"", ""đặt cọc"", tính bằng tiền (cọc 200tr, cọc 500k, ck trước...) cần được cảnh báo mức Medium hoặc High (Scam/PersonalInfo) để sinh viên cẩn thận. Yêu cầu chuyển khoản hay cọc tiền dù số lượng bao nhiêu cũng phải CẢNH BÁO.
- Từ viết tắt tiếng Việt (dm, vcl, vl, đm...) là toxic
- Chỉ phân loại là Safe nếu hoàn toàn không có yếu tố rủi ro.

Trả lời STRICTLY dưới định dạng JSON với CẤU TRÚC SAU (KHÔNG bao gồm markdown \`\`\`json):
{
  ""isSafe"": true/false,
  ""category"": ""Safe|Scam|PersonalInfo|Phishing|Harassment"",
  ""severity"": ""High|Medium|Low"",
  ""alertType"": ""Tên cảnh báo ngắn gọn (VD: POTENTIAL SCAM, TOXIC/HARASSMENT)"",
  ""reason"": ""Giải thích ngắn gọn bằng tiếng Việt tại sao tin nhắn bị flag"",
  ""advice"": ""Lời khuyên bằng tiếng Việt cho người nhận"",
  ""matchedKeywords"": [""từ khóa 1"", ""từ khóa 2""]
}

Nếu an toàn, isSafe = true và các thuộc tính khác rỗng:
{
  ""isSafe"": true,
  ""category"": ""Safe"",
  ""severity"": ""Low"",
  ""alertType"": """",
  ""reason"": """",
  ""advice"": """",
  ""matchedKeywords"": []
}";
        }

        private ModerationResult ParseGroqResponse(string responseJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                
                // Navigate: choices[0].message.content
                var choices = root.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var text = message.GetProperty("content").GetString() ?? "";

                // Clean up just in case
                text = text.Trim();
                if (text.StartsWith("```json")) text = text[7..];
                if (text.StartsWith("```")) text = text[3..];
                if (text.EndsWith("```")) text = text[..^3];
                text = text.Trim();

                using var resultDoc = JsonDocument.Parse(text);
                var result = resultDoc.RootElement;

                bool isSafe = true;
                if (result.TryGetProperty("isSafe", out var safeProp))
                {
                    if (safeProp.ValueKind == JsonValueKind.True || safeProp.ValueKind == JsonValueKind.False)
                        isSafe = safeProp.GetBoolean();
                    else if (safeProp.ValueKind == JsonValueKind.String)
                        isSafe = safeProp.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true;
                }

                if (isSafe)
                {
                    return new ModerationResult { IsSafe = true };
                }

                var matchedKeywords = new List<string>();
                if (result.TryGetProperty("matchedKeywords", out var keywordsElement))
                {
                    foreach (var kw in keywordsElement.EnumerateArray())
                    {
                        matchedKeywords.Add(kw.GetString() ?? "");
                    }
                }

                return new ModerationResult
                {
                    IsSafe = false,
                    Category = result.GetProperty("category").GetString() ?? "Safe",
                    Severity = result.GetProperty("severity").GetString() ?? "Medium",
                    AlertType = result.GetProperty("alertType").GetString() ?? "WARNING",
                    Reason = result.GetProperty("reason").GetString() ?? "",
                    Advice = result.GetProperty("advice").GetString() ?? "",
                    MatchedKeywords = matchedKeywords
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Groq response: {Response}", responseJson);
                throw;
            }
        }
    }
}
