using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniNestBE.DTOs;

namespace UniNestBE.Services
{
    public interface IPremiumCheckService
    {
        /// <summary>
        /// Kiểm tra xem người dùng có tài khoản Premium hợp lệ hay không
        /// </summary>
        bool IsPremiumUser(ClaimsPrincipal user);

        /// <summary>
        /// Trả về response cho biết cần nâng cấp Premium nếu người dùng không phải Premium
        /// </summary>
        IActionResult CheckPremiumAndRedirect(ClaimsPrincipal user, ControllerBase controller);

        /// <summary>
        /// Trả về true nếu người dùng là Premium, false nếu không (không trả về response)
        /// </summary>
        bool IsPremiumOrNull(ClaimsPrincipal user);
    }

    public class PremiumCheckService : IPremiumCheckService
    {
        /// <summary>
        /// Kiểm tra xem người dùng có tài khoản Premium hợp lệ hay không
        /// </summary>
        public bool IsPremiumUser(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            var isPremiumClaim = user.FindFirst("IsPremium")?.Value;
            var premiumExpiryDateClaim = user.FindFirst("PremiumExpiryDate")?.Value;

            if (!bool.TryParse(isPremiumClaim, out var isPremium) || !isPremium)
                return false;

            if (string.IsNullOrEmpty(premiumExpiryDateClaim))
                return false;

            if (!DateTime.TryParse(premiumExpiryDateClaim, out var expiryDate))
                return false;

            return expiryDate > DateTime.UtcNow;
        }

        /// <summary>
        /// Trả về response cho biết cần nâng cấp Premium nếu người dùng không phải Premium
        /// </summary>
        public IActionResult CheckPremiumAndRedirect(ClaimsPrincipal user, ControllerBase controller)
        {
            if (IsPremiumUser(user))
                return null; // Người dùng là Premium, không cần redirect

            // Trả về 403 Forbidden với DTO chứa thông tin redirect
            return controller.StatusCode(StatusCodes.Status403Forbidden, new PremiumRequiredDto
            {
                RequiresPremium = true,
                Message = "Tính năng này yêu cầu tài khoản Premium. Vui lòng nâng cấp để tiếp tục.",
                RedirectUrl = "/premium-info"
            });
        }

        /// <summary>
        /// Trả về true nếu người dùng là Premium, false nếu không
        /// </summary>
        public bool IsPremiumOrNull(ClaimsPrincipal user)
        {
            return IsPremiumUser(user);
        }
    }
}
