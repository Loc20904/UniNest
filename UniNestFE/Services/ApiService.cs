using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace UniNestFE.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(string url, object data);
        Task<T> PutAsync<T>(string url, object data);
        Task<bool> DeleteAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, object data);
        Task<HttpResponseMessage> GetAsync(string url);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public ApiService(HttpClient httpClient, NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Hàm bổ trợ để gắn Token vào Header trước mỗi request
        /// </summary>
        private async Task AddAuthorizationHeader()
        {
            // Lấy token từ localStorage (phụ thuộc vào tên bạn đặt khi Login, ví dụ: "authToken")
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task CheckPremiumRequired(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                try
                {
                    var premiumDto = await response.Content.ReadFromJsonAsync<PremiumRequiredDto>();
                    if (premiumDto?.RequiresPremium == true)
                    {
                        _navigationManager.NavigateTo(premiumDto.RedirectUrl ?? "/premium-info", forceLoad: true);
                    }
                }
                catch { }
            }
            // Nếu trả về 401 (Unauthorized), có thể điều hướng người dùng về trang Login
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("/login");
            }
        }

        public async Task<T> GetAsync<T>(string url)
        {
            await AddAuthorizationHeader(); // Thêm dòng này
            var response = await _httpClient.GetAsync(url);
            await CheckPremiumRequired(response);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PostAsync<T>(string url, object data)
        {
            await AddAuthorizationHeader(); // Thêm dòng this
            var response = await _httpClient.PostAsJsonAsync(url, data);
            await CheckPremiumRequired(response);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PutAsync<T>(string url, object data)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync(url, data);
            await CheckPremiumRequired(response);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<bool> DeleteAsync(string url)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(url);
            await CheckPremiumRequired(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync(url, data);
            await CheckPremiumRequired(response);
            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync(url);
            await CheckPremiumRequired(response);
            return response;
        }
    }

    public class PremiumRequiredDto
    {
        public bool RequiresPremium { get; set; } = true;
        public string Message { get; set; } = "Tính năng này yêu cầu tài khoản Premium. Vui lòng nâng cấp để tiếp tục.";
        public string RedirectUrl { get; set; } = "/premium-info";
    }
}