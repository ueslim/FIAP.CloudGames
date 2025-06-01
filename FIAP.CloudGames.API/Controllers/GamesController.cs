using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FIAP.CloudGames.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var games = await _gameService.GetAllGamesAsync();
            return Ok(games);
        }

        [HttpGet("dapper")]
        public async Task<IActionResult> GetAllGamesWithDapper()
        {
            var games = await _gameService.GetAllGamesWithDapperAsync();
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var game = await _gameService.GetGameByIdAsync(id);
                return Ok(game);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGameDto createGameDto)
        {
            try
            {
                var game = await _gameService.CreateGameAsync(createGameDto);
                return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameDto updateGameDto)
        {
            try
            {
                var game = await _gameService.UpdateGameAsync(id, updateGameDto);
                return Ok(game);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _gameService.DeleteGameAsync(id);
                if (result)
                {
                    return NoContent();
                }
                return NotFound();
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGames(
                [FromQuery] string term,
                [FromQuery] string[] tags,
                [FromQuery] decimal? minPrice,
                [FromQuery] decimal? maxPrice)
        {
            var games = await _gameService.SearchGamesAsync(term, tags, minPrice, maxPrice);
            return Ok(games);
        }

        [HttpPost("purchase")]
        [Authorize]
        public async Task<IActionResult> PurchaseGame([FromBody] PurchaseGameDto purchaseDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var game = await _gameService.PurchaseGameAsync(userId, purchaseDto);
                return Ok(game);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}