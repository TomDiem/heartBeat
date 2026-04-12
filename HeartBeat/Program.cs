using HeartBeat;
using HeartBeat.Config;

var builder = Host.CreateApplicationBuilder(args);

/// Workers
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SummaryWorker>();
/// Singleton para compartir statuses entre workers
builder.Services.AddSingleton<IHealthStatusStore, HealthStatusStore>();

builder.Services.Configure<HealthCheckConfig>(
    builder.Configuration.GetSection("HealthCheck"));
builder.Services.AddHttpClient();


var host = builder.Build();
host.Run();
