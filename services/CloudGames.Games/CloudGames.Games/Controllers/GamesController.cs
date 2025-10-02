using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[ApiController]
[Route("/api/games")]
public class GamesController : ControllerBase
{
    private readonly GamesDbContext _db;
    private readonly IGamesSearch _search;
    private readonly QueueClient _queue;
    public GamesController(GamesDbContext db, IGamesSearch search, QueueClient queue)
    {
        _db = db; _search = search; _queue = queue;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var games = await _db.Games.AsNoTracking().Select(g => new { g.Id, g.Title, g.Price, g.Genre }).ToListAsync();
        return Ok(games);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var game = await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        if (game == null) return NotFound();
        return Ok(game);
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _search.SearchAsync(q);
        return Ok(results);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGameDto dto)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Developer = dto.Developer,
            Publisher = dto.Publisher,
            ReleaseDate = dto.ReleaseDate,
            Genre = dto.Genre,
            Price = dto.Price,
            CoverImageUrl = dto.CoverImageUrl,
            TagsJson = JsonSerializer.Serialize(dto.Tags ?? Array.Empty<string>())
        };
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        await AppendEventAndPublishAsync("GameCreated", game);
        return CreatedAtAction(nameof(Get), new { id = game.Id }, game);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameDto dto)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();
        if (dto.Title != null) game.Title = dto.Title;
        if (dto.Description != null) game.Description = dto.Description;
        if (dto.Developer != null) game.Developer = dto.Developer;
        if (dto.Publisher != null) game.Publisher = dto.Publisher;
        if (dto.ReleaseDate.HasValue) game.ReleaseDate = dto.ReleaseDate.Value;
        if (dto.Genre != null) game.Genre = dto.Genre;
        if (dto.Price.HasValue) game.Price = dto.Price.Value;
        if (dto.CoverImageUrl != null) game.CoverImageUrl = dto.CoverImageUrl;
        if (dto.Tags != null) game.TagsJson = JsonSerializer.Serialize(dto.Tags);
        await _db.SaveChangesAsync();
        await AppendEventAndPublishAsync("GameUpdated", game);
        return Ok(game);
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();
        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
        await AppendEventAndPublishAsync("GameDeleted", new { id });
        return NoContent();
    }

    private async Task AppendEventAndPublishAsync(string type, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        _db.GameEvents.Add(new GameEvent { Id = Guid.NewGuid(), Type = type, Payload = json, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        await _queue.CreateIfNotExistsAsync();
        await _queue.SendMessageAsync(JsonSerializer.Serialize(new { Type = type, Data = payload }));
    }
}

public record CreateGameDto(string Title, string Description, string Developer, string Publisher, DateTime ReleaseDate, string Genre, decimal Price, string CoverImageUrl, string[]? Tags);
public record UpdateGameDto(string? Title, string? Description, string? Developer, string? Publisher, DateTime? ReleaseDate, string? Genre, decimal? Price, string? CoverImageUrl, string[]? Tags);
