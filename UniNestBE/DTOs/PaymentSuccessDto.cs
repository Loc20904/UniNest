namespace UniNestBE.DTOs
{
    public class PaymentSuccessDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PlanType { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
