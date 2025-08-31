namespace MockApi.Models;

public sealed class TokenResponse
{
    public string token_type { get; set; } = "Bearer";
    public int expires_in { get; set; } = 300; 
    public string access_token { get; set; } = "";
}