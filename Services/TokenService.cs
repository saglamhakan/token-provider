using System.Net.Http.Json;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MockApi.Models;

namespace MockApi.Services;

public sealed class TokenOptions
{
    public int HourlyLimit { get; set; } = 5;
    public int SafetySeconds { get; set; } = 30;
}

public sealed class MockEndpoints
{
    public string TokenEndpoint { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}

public sealed class TokenService : ITokenService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly TokenOptions _opt;
    private readonly MockEndpoints _mock;
    private readonly SemaphoreSlim _lock = new(1,1);

    private int _hourlyCalls = 0;
    private DateTime _windowStartUtc = DateTime.UtcNow;

    private const string CacheKey = "ACCESS_TOKEN";
    private const string ExpiryKey = "ACCESS_TOKEN_EXPIRY";

    public TokenService(HttpClient http, IMemoryCache cache,
        IOptions<TokenOptions> opt, IOptions<MockEndpoints> mock)
    {
        _http = http;
        _cache = cache;
        _opt = opt.Value;
        _mock = mock.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue<string>(CacheKey, out var token) &&
            _cache.TryGetValue<DateTime>(ExpiryKey, out var exp) &&
            DateTime.UtcNow < exp.AddSeconds(-_opt.SafetySeconds))
        {
            return token!;
        }

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue<string>(CacheKey, out token) &&
                _cache.TryGetValue<DateTime>(ExpiryKey, out exp) &&
                DateTime.UtcNow < exp.AddSeconds(-_opt.SafetySeconds))
            {
                return token!;
            }

            var now = DateTime.UtcNow;
            if (now - _windowStartUtc >= TimeSpan.FromHours(1))
            {
                _windowStartUtc = now;
                _hourlyCalls = 0;
            }

            if (_hourlyCalls >= _opt.HourlyLimit)
                throw new InvalidOperationException("Hourly token request limit exceeded (5).");

            var reqBody = new Dictionary<string, string>
            {
                ["client_id"] = _mock.ClientId,
                ["client_secret"] = _mock.ClientSecret,
                ["grant_type"] = "client_credentials"
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, _mock.TokenEndpoint)
            { Content = new FormUrlEncodedContent(reqBody) };

            var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();

            var data = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
                       ?? throw new Exception("Token parse failed.");

            _hourlyCalls++;

            token = data.access_token;
            var expiry = DateTime.UtcNow.AddSeconds(data.expires_in);

            _cache.Set(CacheKey, token, expiry);
            _cache.Set(ExpiryKey, expiry, expiry);

            return token!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public (DateTime? ExpiresAtUtc, int HourlyCalls, DateTime WindowStartUtc) DebugInfo()
    {
        DateTime? exp = _cache.TryGetValue<DateTime>(ExpiryKey, out var e) ? e : null;
        return (exp, _hourlyCalls, _windowStartUtc);
    }
}