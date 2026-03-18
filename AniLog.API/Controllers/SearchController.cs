using AniLog.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AniLog.API.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly JikanService _jikanService;

    public SearchController(JikanService jikanService)
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
            return Ok(results);
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
