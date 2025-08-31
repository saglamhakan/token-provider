using System.Net.Http.Json;
using MockApi.Models;

namespace MockApi.Services;

public sealed class OrderClient
{
    private readonly HttpClient _http;
    private readonly ITokenService _tokens;
    private readonly string _ordersUrl;

    public OrderClient(HttpClient http, ITokenService tokens, IConfiguration cfg)
    {
        _http = http;
        _tokens = tokens;
        _ordersUrl = cfg["Mock:OrdersEndpoint"]!;
    }

    public async Task<List<OrderDto>> FetchOrdersAsync(CancellationToken ct = default)
    {
        var token = await _tokens.GetAccessTokenAsync(ct);
        using var req = new HttpRequestMessage(HttpMethod.Get, _ordersUrl);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var list = await resp.Content.ReadFromJsonAsync<List<OrderDto>>(cancellationToken: ct)
                   ?? new List<OrderDto>();
        return list;
    }
}