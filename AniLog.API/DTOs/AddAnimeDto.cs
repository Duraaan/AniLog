using System.ComponentModel.DataAnnotations;
using AniLog.API.Models;

namespace AniLog.API.DTOs;

public class AddAnimeDto
{
    [Required]
    public int MalId { get; set; }

    [Required]
    public AnimeStatus MyStatus { get; set; }

    [Range(0, 10)]
    public decimal MyScore { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int EpisodesWatched { get; set; } = 0;

    public string? MyNotes { get; set; }
}
