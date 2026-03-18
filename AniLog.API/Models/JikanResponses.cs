using System.Text.Json.Serialization;

namespace AniLog.API.Models;

public class JikanSearchResponse
{
    [JsonPropertyName("data")]
    public List<JikanAnimeData> Data { get; set; } = [];
}

public class JikanSingleResponse
{
    [JsonPropertyName("data")]
    public JikanAnimeData? Data { get; set; }
}

public class JikanAnimeData
{
    [JsonPropertyName("mal_id")]
    public int MalId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("title_english")]
    public string? TitleEnglish { get; set; }

    [JsonPropertyName("title_japanese")]
    public string? TitleJapanese { get; set; }

    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    [JsonPropertyName("images")]
    public JikanImages? Images { get; set; }

    [JsonPropertyName("genres")]
    public List<JikanGenre> Genres { get; set; } = [];
}

public class JikanImages
{
    [JsonPropertyName("jpg")]
    public JikanJpg? Jpg { get; set; }
}

public class JikanJpg
{
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;
}

public class JikanGenre
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
