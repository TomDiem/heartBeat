namespace HeartBeat.Config;

public class ServiceEntry
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class HealthCheckConfig
{
    public int IntervalSeconds {get; set; }
    public List<ServiceEntry> Services {get; set;} = new();
}
