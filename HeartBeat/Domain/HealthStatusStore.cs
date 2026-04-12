using System.Collections.Concurrent;

namespace HeartBeat;

public class HealthStatusStore : IHealthStatusStore
{
    private readonly ConcurrentDictionary<string, bool> _statuses = new();

    public Dictionary<string, bool> GetAll()
    {
        return new Dictionary<string, bool>(_statuses);
    }

    public void Update(string serviceName, bool isHealthy)
    {
        _statuses[serviceName] = isHealthy;
    }
}
