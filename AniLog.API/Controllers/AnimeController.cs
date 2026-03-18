using Microsoft.AspNetCore.Mvc;
using AniLog.API.DTOs;
using AniLog.API.Models;
using AniLog.API.Services;

namespace AniLog.API.Controllers;

[ApiController]
[Route("api/anime")]
public class AnimeController : ControllerBase
{
    private readonly AnimeLogService _service;

    public AnimeController(AnimeLogService service)
    {
        _service = service;
    }

    // GET /api/anime
    // GET /api/anime?status=watching
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AnimeStatus? status)
    {
        var result = await _service.GetAllAsync(status);
        return Ok(result);
    }

    // GET /api/anime/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // POST /api/anime
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddAnimeDto dto)
    {
        try
        {
            var result = await _service.AddAnimeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { message = "No se pudo conectar a Jikan API. Intenta en unos segundos." });
        }
    }

    // PUT /api/anime/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAnimeDto dto)
    {
        var result = await _service.UpdateAnimeAsync(id, dto);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // DELETE /api/anime/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAnimeAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
