using System.ComponentModel.DataAnnotations;

namespace AniLog.API.Models;

public class AnimeLog
{
    public int Id { get; set; }

    [Required]
    public int MalId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? TitleJapanese { get; set; }

    public string? Genres { get; set; }

    public int? Episodes { get; set; }

    public decimal MalScore { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, 10)]
    public decimal MyScore { get; set; }

    [Required]
    public AnimeStatus MyStatus { get; set; }

    public int EpisodesWatched { get; set; } = 0;

    public string? MyNotes { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
