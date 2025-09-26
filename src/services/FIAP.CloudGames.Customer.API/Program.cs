using FIAP.CloudGames.Customer.API.Configuration;
using FIAP.CloudGames.WebAPI.Core.Identity;

LoggingConfig.ConfigureBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog + OTLP para logs
builder.ConfigureSerilogWithOpenTelemetry("customer-api");


if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddApiConfiguration(builder.Configuration);

builder.Services.AddMessageBusConfiguration(builder.Configuration);

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSwaggerConfiguration();

builder.Services.RegisterServices();

// OpenTelemetry Tracing + Metrics
builder.Services.AddObservabilityConfiguration(builder.Configuration);

var app = builder.Build();

app.UseSwaggerConfiguration();

app.UseApiConfiguration(app.Environment);

// Logs enriquecidos com user_id
app.UseRequestLogEnrichment();

app.Run();