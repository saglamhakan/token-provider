using Microsoft.AspNetCore.Mvc;
using MockApi.Services;

namespace MockApi.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderClient _client;
    private readonly ITokenService _tokens;

    public OrdersController(OrderClient client, ITokenService tokens)
    {
        _client = client;
        _tokens = tokens;
    }

    [HttpGet("trigger")]
    public async Task<IActionResult> Trigger(CancellationToken ct)
    {
        var list = await _client.FetchOrdersAsync(ct);
        return Ok(list);
    }

    [HttpGet("/token/info")]
    public IActionResult TokenInfo()
    {
        var (exp, calls, start) = _tokens.DebugInfo();
        return Ok(new { expiresAtUtc = exp, hourlyCalls = calls, windowStartUtc = start });
    }
}