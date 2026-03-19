namespace AniLog.API.DTOs;

public class SearchResultDto
{
    public int MalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleEnglish { get; set; }
    public string? TitleJapanese { get; set; }
    public int? Episodes { get; set; }
    public decimal? Score { get; set; }
    public string? ImageUrl { get; set; }
    public string? Genres { get; set; }
}
