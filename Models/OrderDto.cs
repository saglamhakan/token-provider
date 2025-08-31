namespace MockApi.Models;

public sealed class OrderDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Product { get; set; } = "Coffee";
    public int Quantity { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}