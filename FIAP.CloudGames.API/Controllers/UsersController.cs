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
    [Authorize(Roles = "Administrator")]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IGameService _gameService;

        public UsersController(IUserService userService, IGameService gameService)
        {
            _userService = userService;
            _gameService = gameService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(user);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("library")]
        public async Task<IActionResult> GetUserLibrary()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var games = await _gameService.GetUserLibraryAsync(userId);
                return Ok(games);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("library/dapper")]
        public async Task<IActionResult> GetUserLibraryWithDapper()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var games = await _gameService.GetUserLibraryWithDapperAsync(userId);
                return Ok(games);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                await _userService.UpdateUserAsync(id, updateUserDto);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
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
    }
}