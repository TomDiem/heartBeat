using HeartBeat.Domain;

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
                var healthy = statuses.Count(s => s.Value.IsHealthy);
                var total = statuses.Count;
                
                foreach (var (name,status) in statuses)
                {
                    var timeSinceLastCheck = DateTime.UtcNow - status.LastChecked;
                    var isConsiderDown = status.ConsecutiveFailures >= 3;

                    var level = isConsiderDown ? LogLevel.Warning : LogLevel.Information;

                    _logger.Log(level,
                        "HealthCheck {Name} Healthy={IsHealthy} Failures={Failures} LastCheckSecondsAgo={LastCheckAgo}",
                        name,
                        status.IsHealthy,
                        status.ConsecutiveFailures,
                        (int)timeSinceLastCheck.TotalSeconds
                    );
                }

                _logger.LogInformation(
                    "Resumen: {Healthy}/{Total} servicios healthy",
                    healthy, total);

            }
            
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

}
