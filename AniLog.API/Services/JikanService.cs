using System.Text.Json;
using AniLog.API.Models;

namespace AniLog.API.Services;

public class JikanService
{
    private readonly HttpClient _httpClient;

    public JikanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<JikanAnimeData>> SearchAnimeAsync(string query)
    {
        var response = await _httpClient.GetAsync($"anime?q={Uri.EscapeDataString(query)}&limit=10");

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Jikan devolvio {(int)response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JikanSearchResponse>(json);

        return result?.Data ?? [];
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
