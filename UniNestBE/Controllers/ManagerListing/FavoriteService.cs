using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UniNestBE.DTOs;
public class FavoriteService
{
    private readonly HttpClient _http;

    public FavoriteService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> IsSaved(int listingId)
    {
        return await _http.GetFromJsonAsync<bool>($"api/favorites/check/{listingId}");
    }

    public async Task Save(int listingId)
    {
        await _http.PostAsync($"api/favorites/{listingId}", null);
    }

    public async Task Unsave(int listingId)
    {
        await _http.DeleteAsync($"api/favorites/{listingId}");
    }
}