using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniNestBE.Data;
using UniNestBE.DTOs;
using System.Security.Claims;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UniNestBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UniNestDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(IConfiguration configuration, UniNestDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpPost("create-payos-payment")]
        public async Task<ActionResult<dynamic>> CreatePayosPayment([FromBody] CreateCheckoutSessionDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var user = await _dbContext.Users.FindAsync(int.Parse(userId));
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Generate order code
                string orderCode = $"UNINEST{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                string description = request.PlanType == "monthly" 
                    ? "UniNest Premium - Gói Tháng" 
                    : "UniNest Premium - Gói Năm";

                // Prepare PayOS request
                var payosRequest = new
                {
                    orderCode = orderCode,
                    amount = request.Amount,
                    description = description,
                    buyerName = user.FullName,
                    buyerEmail = user.Email,
                    buyerPhone = user.PhoneNumber,
                    buyerAddress = user.CurrentAddress ?? "Vietnam",
                    items = new[]
                    {
                        new
                        {
                            name = description,
                            quantity = 1,
                            price = request.Amount
                        }
                    },
                    cancelUrl = _configuration["PayOS:CancelUrl"],
                    returnUrl = _configuration["PayOS:SuccessUrl"] + $"?orderCode={orderCode}&planType={request.PlanType}&userId={userId}"
                };

                // Create PayOS payment link
                using (var client = _httpClientFactory.CreateClient())
                {
                    string clientId = _configuration["PayOS:ClientId"];
                    string apiKey = _configuration["PayOS:ApiKey"];
                    
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var content = new StringContent(
                        JsonSerializer.Serialize(payosRequest),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync(
                        "https://api.payos.vn/v1/Payment/Create",
                        content
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(responseBody))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("data", out var data) && 
                                data.TryGetProperty("checkoutUrl", out var checkoutUrl))
                            {
                                return Ok(new
                                {
                                    checkoutUrl = checkoutUrl.GetString(),
                                    orderCode = orderCode
                                });
                            }
                        }
                    }

                    return BadRequest("Failed to create PayOS payment link");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("confirm-payos-payment")]
        public async Task<ActionResult<PaymentSuccessDto>> ConfirmPayosPayment([FromBody] string orderCode)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                // Get payment info from PayOS
                using (var client = _httpClientFactory.CreateClient())
                {
                    string clientId = _configuration["PayOS:ClientId"];
                    string apiKey = _configuration["PayOS:ApiKey"];
                    
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var response = await client.GetAsync(
                        $"https://api.payos.vn/v1/Payment/GetPaymentLinkInformation?orderCode={orderCode}"
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest("Payment not found");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(responseBody))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("data", out var data) && 
                            data.TryGetProperty("status", out var status))
                        {
                            // Check if payment is successful (status = "PAID")
                            if (status.GetString() != "PAID")
                            {
                                return BadRequest("Payment not completed");
                            }

                            var user = await _dbContext.Users.FindAsync(int.Parse(userId));
                            if (user == null)
                            {
                                return NotFound("User not found");
                            }

                            // Determine plan type from order code or request
                            string planType = orderCode.Contains("YEARLY") ? "yearly" : "monthly";

                            // Update user premium status
                            user.IsPremium = true;
                            
                            if (planType == "monthly")
                            {
                                user.PremiumExpiryDate = DateTime.UtcNow.AddMonths(1);
                            }
                            else
                            {
                                user.PremiumExpiryDate = DateTime.UtcNow.AddYears(1);
                            }

                            _dbContext.Users.Update(user);
                            await _dbContext.SaveChangesAsync();

                            return Ok(new PaymentSuccessDto
                            {
                                Success = true,
                                Message = $"Premium subscription activated for {planType} plan",
                                PlanType = planType,
                                ExpiresAt = user.PremiumExpiryDate ?? DateTime.UtcNow.AddMonths(1)
                            });
                        }
                    }
                }

                return BadRequest("Unable to verify payment status");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
