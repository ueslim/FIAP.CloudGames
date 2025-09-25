


using FIAP.CloudGames.Identity.API.Configuration;
using FIAP.CloudGames.WebAPI.Core.Identity;

var builder = WebApplication.CreateBuilder(args);

// Optional: explicitly add User Secrets in Dev (CreateBuilder already loads appsettings, env vars, etc.)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// === Services (migrated from Startup.ConfigureServices) ===
builder.Services.AddApiConfiguration();

builder.Services.AddMessageBusConfiguration(builder.Configuration);

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddSwaggerConfiguration();


var app = builder.Build();

// === Middleware (migrated from Startup.Configure) ===
app.UseSwaggerConfiguration();

app.UseApiConfiguration(app.Environment);

app.Run();
