using Microsoft.AspNetCore.Mvc;
using MockApi.Models;

namespace MockApi.Controllers;

[ApiController]
[Route("mock/oauth")]
public sealed class MockAuthController : ControllerBase
{
    [HttpPost("token")]
    public IActionResult Token([FromForm] string client_id, [FromForm] string client_secret, [FromForm] string grant_type)
    {
        if (client_id != "demo" || client_secret != "demo") return Unauthorized();

        return Ok(new TokenResponse
        {
            token_type = "Bearer",
            expires_in = 300, 
            access_token = Guid.NewGuid().ToString("N")
        });
    }
}