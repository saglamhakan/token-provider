using Microsoft.Extensions.Options;

namespace MockApi.Workers;

public sealed class PollingOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 5;
}

public sealed class OrderPoller : BackgroundService
{
    private readonly ILogger<OrderPoller> _log;
    private readonly Services.OrderClient _client;
    private readonly PollingOptions _opt;

    public OrderPoller(ILogger<OrderPoller> log, Services.OrderClient client, IOptions<PollingOptions> opt)
    {
        _log = log;
        _client = client;
        _opt = opt.Value;
    }

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    if (!_opt.Enabled) return;

    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

    var delay = TimeSpan.FromMinutes(_opt.IntervalMinutes);

    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            var orders = await _client.FetchOrdersAsync(stoppingToken);
            _log.LogInformation("Fetched {Count} orders at {Time}.", orders.Count, DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Order polling failed.");
        }

        await Task.Delay(delay, stoppingToken);
    }
}
}