namespace UniNestBE.DTOs
{
    public class CreateCheckoutSessionDto
    {
        public string PlanType { get; set; } // "monthly" or "yearly"
        public int Amount { get; set; } // Amount in cents (Stripe format)
        public string PriceId { get; set; } // Stripe Price ID
    }
}
