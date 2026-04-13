using System.Collections.Concurrent;

namespace HeartBeat.Domain;

public class HealthStatusStore : IHealthStatusStore
{
    private readonly ConcurrentDictionary<string, HealthStatus> _statuses = new();

    public Dictionary<string, HealthStatus> GetAll()
    {
        return new Dictionary<string, HealthStatus>(_statuses);
    }

    public void Update(string serviceName, bool isHealthy)
    {
        _statuses.AddOrUpdate(serviceName,
        // si no existe
        _ => new HealthStatus
        {
            IsHealthy = isHealthy,
            LastChecked = DateTime.UtcNow,
            ConsecutiveFailures = isHealthy ? 0 : 1,
        },
        // si ya existe
        (_, existing) =>
        {
            if(isHealthy)
            {
                existing.ConsecutiveFailures = 0;
            }
            else
            {
                existing.ConsecutiveFailures++;
            }

            existing.IsHealthy = isHealthy;
            existing.LastChecked = DateTime.UtcNow;

            return existing;
        });
    }
}
