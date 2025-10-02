using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) { _userService = userService; }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        var user = await _userService.GetMeAsync(Guid.Parse(idClaim));
        return Ok(user);
    }
}
