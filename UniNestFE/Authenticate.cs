using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 1. Tạo thông tin user giả
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Test User"), // Tên hiển thị
            new Claim(ClaimTypes.NameIdentifier, "1"), // ID user
            new Claim(ClaimTypes.Role, "Admin") // Giả làm Admin
        };

        // 2. QUAN TRỌNG NHẤT: Thêm chuỗi "FakeAuth" vào tham số thứ 2
        // Có chuỗi này -> Blazor hiểu là ĐÃ ĐĂNG NHẬP (IsAuthenticated = true)
        // Không có chuỗi này -> Blazor hiểu là KHÁCH (IsAuthenticated = false)
        var identity = new ClaimsIdentity(claims, "FakeAuth");

        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }
}