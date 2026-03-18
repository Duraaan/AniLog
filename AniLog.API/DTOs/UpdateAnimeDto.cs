using System.ComponentModel.DataAnnotations;
using AniLog.API.Models;

namespace AniLog.API.DTOs;

public class UpdateAnimeDto
{
    public AnimeStatus? MyStatus { get; set; }

    [Range(0, 10)]
    public decimal? MyScore { get; set; }

    [Range(0, int.MaxValue)]
    public int? EpisodesWatched { get; set; }

    public string? MyNotes { get; set; }
}
