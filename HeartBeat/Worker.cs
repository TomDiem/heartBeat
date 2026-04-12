using HeartBeat.Config;
using Microsoft.Extensions.Options;

namespace HeartBeat;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly HealthCheckConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHealthStatusStore _store;
    public Worker(
        ILogger<Worker> logger,
        IOptions<HealthCheckConfig> config,
        IHttpClientFactory httpClientFactory,
        IHealthStatusStore store)
    {
        _logger = logger;
        _config = config.Value;
        _httpClientFactory = httpClientFactory;
        _store = store;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var service in _config.Services)
            {
                await CheckServiceAsync(service);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_config.IntervalSeconds), 
                stoppingToken);
        }
    }


    private async Task CheckServiceAsync(ServiceEntry service)
    {
        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetAsync(service.Url);
            var isHealthy = response.IsSuccessStatusCode;
            _store.Update(service.Name, isHealthy);

            _logger.LogInformation(
                "{Status} {Name} ({Url}) - {StatusCode}",
                isHealthy ? "✅" : "⚠️",
                service.Name, service.Url, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _store.Update(service.Name, false);

            _logger.LogWarning(
                "❌ {Name} ({Url}) - Unhealthy: {Error}",
                service.Name, service.Url, ex.Message);
        }
    }
}
