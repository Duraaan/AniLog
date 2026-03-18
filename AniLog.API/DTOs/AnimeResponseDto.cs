using AniLog.API.Models;

namespace AniLog.API.DTOs;

public class AnimeResponseDto
{
    public int Id { get; set; }
    public int MalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleJapanese { get; set; }
    public string? Genres { get; set; }
    public int? Episodes { get; set; }
    public decimal MalScore { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public decimal MyScore { get; set; }
    public AnimeStatus MyStatus { get; set; }
    public int EpisodesWatched { get; set; }
    public string? MyNotes { get; set; }
    public DateTime AddedAt { get; set; }
}
