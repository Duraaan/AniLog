using System.Text.Json;
using AniLog.API.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AniLog.API.Services;

public class JikanService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public JikanService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<List<JikanAnimeData>> SearchAnimeAsync(string query)
    {
        var cacheKey = $"search:{query.ToLowerInvariant().Trim()}";

        if (_cache.TryGetValue(cacheKey, out List<JikanAnimeData>? cached))
            return cached!;

        var response = await _httpClient.GetAsync($"anime?q={Uri.EscapeDataString(query)}&limit=10");

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Jikan devolvio {(int)response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JikanSearchResponse>(json);
        var data = result?.Data ?? [];

        _cache.Set(cacheKey, data, CacheDuration);

        return data;
    }

    public async Task<JikanAnimeData?> GetAnimeByIdAsync(int malId)
    {
        var response = await _httpClient.GetAsync($"anime/{malId}");

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Jikan devolvio {(int)response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JikanSingleResponse>(json);

        return result?.Data;
    }
}
