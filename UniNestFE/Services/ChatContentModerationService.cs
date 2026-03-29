using System.Text.RegularExpressions;

namespace UniNestFE.Services
{
    public class ScamAlertInfo
    {
        public string AlertType { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Advice { get; set; } = "";
        public string Severity { get; set; } = "Medium"; // High, Medium, Low
        public List<string> MatchedKeywords { get; set; } = new();
        public bool IsDismissed { get; set; } = false;
    }

    public class ChatContentModerationService
    {
        private readonly List<ModerationRule> _rules = new()
{
    // ==========================================================
    // === HIGH SEVERITY — Scam (Tiền bạc / Lừa đảo cọc) ===
    // ==========================================================
    new ModerationRule
    {
        Patterns = new[] { 
            // Tiếng Việt
            @"cọc gấp", @"đặt cọc ngay", @"chuyển khoản gấp", @"chuyển cọc gấp", @"chuyển tiền giữ chỗ", 
            @"cọc trước", @"đóng cọc", @"đóng tiền trước", @"ck trước", @"chuyển lẹ", @"cọc liền",
            // Tiếng Anh
            @"deposit urgently", @"transfer immediately", @"wire transfer", @"send deposit", 
            @"pay upfront", @"urgent deposit", @"secure the room now", @"pay immediately" 
        },
        AlertType = "POTENTIAL SCAM",
        Reason = "Tin nhắn chứa các từ khóa hối thúc nộp tiền ('{keyword}'). Đây là một chiến thuật lừa đảo rất phổ biến.",
        Advice = "Tuyệt đối không chuyển tiền cọc khi chưa xem phòng trực tiếp và ký hợp đồng rõ ràng.",
        Severity = "High",
        Category = "Scam"
    },
    new ModerationRule
    {
        Patterns = new[] { 
            // Tiếng Việt
            @"gửi tiền trước", @"thanh toán trước", @"trả tiền trước", @"chuyển tiền trước", 
            @"đóng trước", @"đưa tiền trước", @"chuyển trước \d+%", @"cọc \d+ tháng",
            // Tiếng Anh
            @"pay in advance", @"advance payment", @"send money first", @"pay beforehand", @"prepay" 
        },
        AlertType = "POTENTIAL SCAM",
        Reason = "Yêu cầu thanh toán trước khi xem phòng ('{keyword}'). Chủ trọ uy tín hiếm khi yêu cầu điều này qua chat.",
        Advice = "Chỉ thanh toán sau khi đã xác minh tính xác thực của phòng trọ và người cho thuê.",
        Severity = "High",
        Category = "Scam"
    },
    new ModerationRule
    {
        Patterns = new[] { 
            // Tiếng Việt
            @"chuyển khoản", @"số tài khoản", @"\bstk\b", @"\bmomo\b", @"banking", 
            @"zalopay", @"vnpay", @"quét mã qr", @"mã qr", @"viettelpay", @"shopeepay",
            // Tiếng Anh
            @"bank account", @"account number", @"wire money", @"paypal", @"crypto", @"bitcoin" 
        },
        AlertType = "PAYMENT REQUEST",
        Reason = "Tin nhắn nhắc đến phương thức thanh toán ('{keyword}'). Hãy cẩn thận với các giao dịch tài chính qua mạng.",
        Advice = "Chỉ thực hiện thanh toán qua các kênh chính thức hoặc khi gặp trực tiếp. Lưu lại mọi bằng chứng giao dịch.",
        Severity = "High",
        Category = "PersonalInfo"
    },

    // ==========================================================
    // === MEDIUM SEVERITY — Urgency Pressure (Gây áp lực/FOMO) ===
    // ==========================================================
    new ModerationRule
    {
        Patterns = new[] { 
            // Tiếng Việt
            @"giữ phòng", @"hết phòng", @"nhiều người hỏi", @"sắp hết", @"còn 1 phòng", 
            @"chốt lẹ", @"không là mất", @"người khác lấy", @"có người cọc", @"nhiều sinh viên đang xem",
            // Tiếng Anh
            @"last room", @"almost gone", @"many people asking", @"book fast", 
            @"hurry up", @"someone else is looking", @"high demand" 
        },
        AlertType = "URGENCY PRESSURE",
        Reason = "Người gửi đang dùng chiến thuật tạo áp lực tâm lý ('{keyword}') để ép bạn quyết định nhanh.",
        Advice = "Hãy bình tĩnh xem xét. Một phòng trọ tốt vẫn sẽ còn đó sau khi bạn kiểm tra kỹ thông tin.",
        Severity = "Medium",
        Category = "Scam"
    },

    // ==========================================================
    // === MEDIUM SEVERITY — Personal Info Request (Trích xuất thông tin cá nhân) ===
    // ==========================================================
    new ModerationRule
    {
        Patterns = new[] { 
            // Tiếng Việt
            @"cmnd", @"cccd", @"căn cước", @"chứng minh nhân dân", @"số chứng minh", 
            @"hộ chiếu", @"chụp hai mặt", @"gửi \bID\b", @"chụp cccd", @"mã otp", @"mật khẩu",
            // Tiếng Anh
            @"id card", @"passport", @"social security", @"ssn", @"driver's license", 
            @"otp", @"password", @"verification code", @"send a picture of your id" 
        },
        AlertType = "PERSONAL INFO REQUEST",
        Reason = "Yêu cầu cung cấp giấy tờ tùy thân hoặc thông tin bảo mật ('{keyword}'). Tuyệt đối không gửi qua chat.",
        Advice = "Chỉ cung cấp giấy tờ cá nhân khi gặp trực tiếp để làm hợp đồng thuê nhà chính thức.",
        Severity = "Medium",
        Category = "PersonalInfo"
    },

    // ==========================================================
    // === LOW/MEDIUM SEVERITY — Suspicious Links (Link đáng ngờ/Phishing) ===
    // ==========================================================
    new ModerationRule
    {
        Patterns = new[] { 
            // Tiếng Việt & Tiếng Anh
            @"bit\.ly", @"tinyurl", @"click vào link", @"nhấn vào đây", @"link rút gọn", 
            @"đăng nhập vào link", @"vào nhóm zalo", @"link nhóm", @"nhấn link", 
            @"click the link", @"click here", @"shortlink", @"login link", @"join the group", @"shorte\.st" 
        },
        AlertType = "SUSPICIOUS LINK",
        Reason = "Tin nhắn chứa liên kết rút gọn hoặc mời gọi bấm vào link lạ ('{keyword}').",
        Advice = "Không bấm vào các đường link không rõ nguồn gốc. Có thể là trang web giả mạo để đánh cắp tài khoản.",
        Severity = "Medium",
        Category = "Phishing"
    },

    // ==========================================================
    // === HIGH SEVERITY — Toxic Language / Harassment (Ngôn từ độc hại - Phục vụ FE-06) ===
    // ==========================================================
    new ModerationRule
    {
        // Lưu ý: Các từ ngữ nhạy cảm ở đây được liệt kê để regex bắt được. 
        // Trong thực tế, bạn có thể tạo một bảng riêng trong Database để dễ update hơn.
        Patterns = new[] { 
            // Tiếng Việt (Viết tắt & Đầy đủ)
            @"\bdm\b",@"\bđm\b", @"\bvcl\b", @"\bvl\b", @"\bđệt\b", @"\bđmm\b", 
            @"chó đẻ", @"lừa đảo", @"thằng điên", @"con đĩ", @"địt",
            // Tiếng Anh
            @"\bfuck\b", @"\bshit\b", @"\bbitch\b", @"\basshole\b", @"\bcunt\b", @"\bslut\b"
        },
        AlertType = "TOXIC/HARASSMENT",
        Reason = "Tin nhắn chứa ngôn từ không phù hợp hoặc vi phạm tiêu chuẩn cộng đồng UniNest ('{keyword}').",
        Advice = "UniNest hướng tới cộng đồng sinh viên văn minh. Vui lòng giữ thái độ tôn trọng khi giao tiếp.",
        Severity = "High",
        Category = "Harassment"
    }
};

        /// <summary>
        /// Scans a message content and returns a ScamAlertInfo if dangerous patterns are detected.
        /// Returns null if the message is safe.
        /// </summary>
        public ScamAlertInfo? ScanMessage(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            var lowerContent = content.ToLowerInvariant();
            ScamAlertInfo? highestAlert = null;
            int highestPriority = -1;

            foreach (var rule in _rules)
            {
                foreach (var pattern in rule.Patterns)
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    var match = regex.Match(content);

                    if (match.Success)
                    {
                        int priority = rule.Severity switch
                        {
                            "High" => 3,
                            "Medium" => 2,
                            "Low" => 1,
                            _ => 0
                        };

                        if (priority > highestPriority)
                        {
                            highestPriority = priority;
                            highestAlert = new ScamAlertInfo
                            {
                                AlertType = rule.AlertType,
                                Reason = rule.Reason.Replace("{keyword}", match.Value),
                                Advice = rule.Advice,
                                Severity = rule.Severity,
                                MatchedKeywords = new List<string> { match.Value }
                            };
                        }
                        else if (priority == highestPriority && highestAlert != null)
                        {
                            // Accumulate matched keywords for same priority
                            if (!highestAlert.MatchedKeywords.Contains(match.Value))
                            {
                                highestAlert.MatchedKeywords.Add(match.Value);
                            }
                        }
                    }
                }
            }

            return highestAlert;
        }

        private class ModerationRule
        {
            public string[] Patterns { get; set; } = Array.Empty<string>();
            public string AlertType { get; set; } = "";
            public string Reason { get; set; } = "";
            public string Advice { get; set; } = "";
            public string Severity { get; set; } = "Medium";
            public string Category { get; set; } = "";
        }
    }
}
