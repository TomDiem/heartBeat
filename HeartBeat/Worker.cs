using HeartBeat.Config;
using HeartBeat.Domain;
using Microsoft.Extensions.Options;

namespace HeartBeat;

public class Worker : BackgroundService
{
    private const int MAX_PARALLELISM = 5;
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
        var semaphore = new SemaphoreSlim(MAX_PARALLELISM);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var tasks = _config.Services.Select(async service =>
            {
                await semaphore.WaitAsync(stoppingToken);

                try
                {
                    await CheckServiceAsync(service, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking service {Name}", service.Name);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var start = DateTime.UtcNow;

            await Task.WhenAll(tasks);

            var duration = DateTime.UtcNow - start;
            _logger.LogInformation("Ciclo completado en {Duration} ms",
                (int)duration.TotalMilliseconds);

            await Task.Delay(
                TimeSpan.FromSeconds(_config.IntervalSeconds), 
                stoppingToken);
        }
    }


    private async Task CheckServiceAsync(ServiceEntry service, CancellationToken stoppingToken)
    {
        var client = _httpClientFactory.CreateClient("health");
        
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            var response = await client.GetAsync(service.Url, cts.Token);
            var isHealthy = response.IsSuccessStatusCode;
            _store.Update(service.Name, isHealthy);

            _logger.LogInformation(
            "HealthCheck {Name} {Url} StatusCode={StatusCode} Healthy={IsHealthy}",
            service.Name, service.Url, (int)response.StatusCode, isHealthy);
        }
        catch (TaskCanceledException)
        {
            _store.Update(service.Name, false);

            _logger.LogWarning(
                "HealthCheck {Name} {Url} TIMEOUT", service.Name, service.Url);
        }
        catch (Exception ex)
        {
            _store.Update(service.Name, false);

            _logger.LogWarning(ex,
                "HealthCheck {Name} {Url} TIMEOUT",
                service.Name, service.Url);
        }
    }
}
