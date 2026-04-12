namespace HeartBeat;

public interface IHealthStatusStore
{
    void Update(string serviceName, bool isHealthy);
    Dictionary<string, bool> GetAll();
}
