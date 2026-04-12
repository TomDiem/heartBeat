using System.IO.Compression;
using HeartBeat.Config;
using Microsoft.Extensions.Options;

namespace HeartBeat;

public class SummaryWorker : BackgroundService
{
    private readonly ILogger<SummaryWorker> _logger;
    private readonly IHealthStatusStore _store;

    public SummaryWorker(
        ILogger<SummaryWorker> logger,
        IHealthStatusStore store)
    {
        _logger = logger;
        _store = store;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            var statuses = _store.GetAll();

            if(statuses.Count == 0)
            {
                _logger.LogInformation("Esperando primer check...");
            }
            else
            {
                var healthy = statuses.Count(s => s.Value);
                var total = statuses.Count;
                
                foreach (var status in statuses)
                {
                    _logger.LogInformation(
                    "  {Status} {Name}",
                    status.Value ? "🟢" : "🔴", status.Key);
                }

                _logger.LogInformation(
                    "📊 Resumen: {Healthy}/{Total} servicios healthy",
                    healthy, total);

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

        }
    }

}
