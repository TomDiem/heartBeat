namespace HeartBeat.Domain;

public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public DateTime LastChecked { get; set; }
    public int ConsecutiveFailures { get; set; }
}
