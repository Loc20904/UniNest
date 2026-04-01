namespace UniNestBE.DTOs
{
    public class PremiumRequiredDto
    {
        public bool RequiresPremium { get; set; } = true;
        public string Message { get; set; } = "Tính năng này yêu cầu tài khoản Premium. Vui lòng nâng cấp để tiếp tục.";
        public string RedirectUrl { get; set; } = "/premium-info";
    }
}
