using AniLog.API.DTOs;
using AniLog.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AniLog.API.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IJikanService _jikanService;

    public SearchController(IJikanService jikanService)
    {
        _jikanService = jikanService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("El parametro 'q' es requerido.");

        try
        {
            var results = await _jikanService.SearchAnimeAsync(q);
            var dtos = results.Select(a => new SearchResultDto
            {
                MalId = a.MalId,
                Title = a.Title,
                TitleEnglish = a.TitleEnglish,
                TitleJapanese = a.TitleJapanese,
                Episodes = a.Episodes,
                Score = a.Score,
                ImageUrl = a.Images?.Jpg?.ImageUrl,
                Genres = a.Genres is { Count: > 0 }
                    ? string.Join(", ", a.Genres.Select(g => g.Name))
                    : null,
            });
            return Ok(dtos);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            return StatusCode(503, "Jikan no disponible por rate limit, intenta en unos segundos.");
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "No se pudo conectar con Jikan API.");
        }
    }
}
