using AniLog.API.Models;

namespace AniLog.API.Services;

public interface IJikanService
{
    Task<List<JikanAnimeData>> SearchAnimeAsync(string query);
    Task<JikanAnimeData?> GetAnimeByIdAsync(int malId);
}
