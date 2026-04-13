namespace HeartBeat.Domain;

public interface IHealthStatusStore
{
    void Update(string serviceName, bool isHealthy);
    Dictionary<string, HealthStatus> GetAll();
}
