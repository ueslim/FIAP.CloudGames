using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FIAP.CloudGames.Core.Observability;

public static class ObservabilityConfig
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration config, string serviceName)
    {
        var baseEndpoint = config["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4318";
        var tracesEndpoint = $"{baseEndpoint.TrimEnd('/')}/v1/traces";
        var metricsEndpoint = $"{baseEndpoint.TrimEnd('/')}/v1/metrics";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName)
                .AddAttributes(new[]
                {
                     new KeyValuePair<string, object>("deployment.environment", config["ASPNETCORE_ENVIRONMENT"] ?? "Development"),
                     new KeyValuePair<string, object>("service.instance.id", Environment.MachineName)
                }))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddOtlpExporter(o =>
                {
                    o.Protocol = OtlpExportProtocol.HttpProtobuf; // HTTP
                    o.Endpoint = new Uri(tracesEndpoint);         // .../v1/traces
                }))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Protocol = OtlpExportProtocol.HttpProtobuf; // HTTP
                    o.Endpoint = new Uri(metricsEndpoint);        // .../v1/metrics
                }));

        return services;
    }
}