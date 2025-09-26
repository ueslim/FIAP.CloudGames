using FIAP.CloudGames.Catalog.API.Configuration;
using FIAP.CloudGames.Catalog.API.Data;
using FIAP.CloudGames.WebAPI.Core.Identity;

LoggingConfig.ConfigureBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog + OTLP para logs
builder.ConfigureSerilogWithOpenTelemetry("cart-api");

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddApiConfiguration(builder.Configuration);

builder.Services.AddMessageBusConfiguration(builder.Configuration);

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddSwaggerConfiguration();

builder.Services.RegisterServices();

// OpenTelemetry Tracing + Metrics
builder.Services.AddObservabilityConfiguration(builder.Configuration);

var app = builder.Build();

//SEED

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    await CatalogContextSeed.EnsureSeedProducts(context);
}

app.UseSwaggerConfiguration();

app.UseApiConfiguration(app.Environment);

// Logs enriquecidos com user_id
app.UseRequestLogEnrichment();

app.Run();