using Microsoft.AspNetCore.Mvc;
using MockApi.Models;

namespace MockApi.Controllers;

[ApiController]
[Route("mock/vendor")]
public sealed class MockVendorController : ControllerBase
{
    [HttpGet("orders")]
    public IActionResult Orders()
    {
        var auth = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer "))
            return Unauthorized(new { message = "Missing or invalid Bearer token." });

        var list = Enumerable.Range(1, 3).Select(i => new OrderDto
        {
            Id = Guid.NewGuid().ToString("N"),
            Product = $"Product-{i}",
            Quantity = i,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        return Ok(list);
    }
}