using System.Threading.Tasks;

namespace MockApi.Services;

public interface ITokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    (DateTime? ExpiresAtUtc, int HourlyCalls, DateTime WindowStartUtc) DebugInfo();
}