using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;

namespace FIAP.CloudGames.Payment.API.Configuration
{
    public static class LoggingConfig
    {
        // Boot mínimo (antes do builder) – console only
        public static void ConfigureBootstrapLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .WriteTo.Console()
                .CreateLogger();
        }

        // Configura Serilog definitivo + envia logs ao SigNoz via OTLP
        public static void ConfigureSerilogWithOpenTelemetry(this WebApplicationBuilder builder, string serviceName)
        {
            var baseEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4318";
            var logsEndpoint = $"{baseEndpoint.TrimEnd('/')}/v1/logs";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithSpan() // trace_id / span_id nos logs
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = logsEndpoint;
                    options.Protocol = OtlpProtocol.HttpProtobuf;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["deployment.environment"] = builder.Environment.EnvironmentName,
                        ["service.instance.id"] = Environment.MachineName
                    };
                })
                .CreateLogger();

            builder.Logging.ClearProviders(); // evita logs duplicados
            builder.Host.UseSerilog(); // integra Serilog ao host
        }

        // (Opcional) Enriquecer cada request com user_id
        public static void UseRequestLogEnrichment(this IApplicationBuilder app)
        {
            app.Use(async (ctx, next) =>
            {
                using (Serilog.Context.LogContext.PushProperty("user_id", ctx.User?.FindFirst("sub")?.Value))
                {
                    await next();
                }
            });
        }
    }
}
