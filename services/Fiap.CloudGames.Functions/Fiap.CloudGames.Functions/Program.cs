using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("Fiap.CloudGames.Functions"))
            .WithTracing(t => t.AddAzureMonitorTraceExporter());
    })
    .Build();

host.Run();
