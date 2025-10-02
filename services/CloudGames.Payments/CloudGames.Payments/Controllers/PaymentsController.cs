using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("/api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentsController(IPaymentService service) { _service = service; }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resp = await _service.InitiateAsync(userId, dto);
        return Accepted(resp);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Status(Guid id)
    {
        var resp = await _service.GetStatusAsync(id);
        return Ok(resp);
    }
}
