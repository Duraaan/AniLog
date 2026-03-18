using Microsoft.EntityFrameworkCore;
using AniLog.API.Data;
using AniLog.API.DTOs;
using AniLog.API.Models;

namespace AniLog.API.Services;

public class AnimeLogService
{
    private readonly AppDbContext _db;
    private readonly JikanService _jikan;

    public AnimeLogService(AppDbContext db, JikanService jikan)
    {
        _db = db;
        _jikan = jikan;
    }

    public async Task<List<AnimeResponseDto>> GetAllAsync(AnimeStatus? status)
    {
        var query = _db.AnimeLogs.AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.MyStatus == status.Value);

        return await query
            .OrderByDescending(a => a.AddedAt)
            .Select(a => ToDto(a))
            .ToListAsync();
    }

    public async Task<AnimeResponseDto?> GetByIdAsync(int id)
    {
        var anime = await _db.AnimeLogs.FindAsync(id);
        return anime is null ? null : ToDto(anime);
    }

    public async Task<AnimeResponseDto> AddAnimeAsync(AddAnimeDto dto)
    {
        bool exists = await _db.AnimeLogs.AnyAsync(a => a.MalId == dto.MalId);
        if (exists)
            throw new InvalidOperationException($"El anime con MalId {dto.MalId} ya esta en tu lista.");

        var jikanData = await _jikan.GetAnimeByIdAsync(dto.MalId)
            ?? throw new KeyNotFoundException($"No se encontro el anime con MalId {dto.MalId} en Jikan.");

        var anime = new AnimeLog
        {
            MalId = dto.MalId,
            Title = jikanData.TitleEnglish ?? jikanData.Title,
            TitleJapanese = jikanData.TitleJapanese,
            Genres = jikanData.Genres is { Count: > 0 }
                ? string.Join(", ", jikanData.Genres.Select(g => g.Name))
                : null,
            Episodes = jikanData.Episodes,
            MalScore = jikanData.Score ?? 0,
            ImageUrl = jikanData.Images?.Jpg?.ImageUrl ?? string.Empty,
            MyScore = dto.MyScore,
            MyStatus = dto.MyStatus,
            EpisodesWatched = dto.EpisodesWatched,
            MyNotes = dto.MyNotes,
            AddedAt = DateTime.UtcNow
        };

        _db.AnimeLogs.Add(anime);
        await _db.SaveChangesAsync();

        return ToDto(anime);
    }

    public async Task<AnimeResponseDto?> UpdateAnimeAsync(int id, UpdateAnimeDto dto)
    {
        var anime = await _db.AnimeLogs.FindAsync(id);
        if (anime is null) return null;

        if (dto.MyStatus.HasValue) anime.MyStatus = dto.MyStatus.Value;
        if (dto.MyScore.HasValue) anime.MyScore = dto.MyScore.Value;
        if (dto.EpisodesWatched.HasValue) anime.EpisodesWatched = dto.EpisodesWatched.Value;
        if (dto.MyNotes is not null) anime.MyNotes = dto.MyNotes;

        await _db.SaveChangesAsync();
        return ToDto(anime);
    }

    public async Task<bool> DeleteAnimeAsync(int id)
    {
        var anime = await _db.AnimeLogs.FindAsync(id);
        if (anime is null) return false;

        _db.AnimeLogs.Remove(anime);
        await _db.SaveChangesAsync();
        return true;
    }

    private static AnimeResponseDto ToDto(AnimeLog a) => new()
    {
        Id = a.Id,
        MalId = a.MalId,
        Title = a.Title,
        TitleJapanese = a.TitleJapanese,
        Genres = a.Genres,
        Episodes = a.Episodes,
        MalScore = a.MalScore,
        ImageUrl = a.ImageUrl,
        MyScore = a.MyScore,
        MyStatus = a.MyStatus,
        EpisodesWatched = a.EpisodesWatched,
        MyNotes = a.MyNotes,
        AddedAt = a.AddedAt
    };
}
