using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins("http://localhost:4200", "https://*.azurestaticapps.net")
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddDbContext<GamesDbContext>(opt =>
    opt.UseSqlServer(config.GetConnectionString("GamesDb")));

// Queue publisher for game events
builder.Services.AddSingleton(sp =>
{
    var cs = config.GetConnectionString("Storage") ?? "UseDevelopmentStorage=true";
    return new QueueClient(cs, config["Queues:Games"] ?? "games-events");
});

var jwtKey = config["Jwt:Secret"] ?? "replace-me";
var jwtIssuer = config["Jwt:Issuer"] ?? "Fiap.CloudGames.Users";
var jwtAudience = config["Jwt:Audience"] ?? "Fiap.CloudGames.Clients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CloudGames Games API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Fiap.CloudGames.Games"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation());

// Search abstraction registration
var endpointStr = config["Search:Endpoint"] ?? string.Empty;
var apiKeyStr = config["Search:ApiKey"] ?? string.Empty;
if (Uri.TryCreate(endpointStr, UriKind.Absolute, out var endpoint) && endpoint.Scheme == Uri.UriSchemeHttps && !string.IsNullOrWhiteSpace(apiKeyStr))
{
    builder.Services.AddSingleton<IGamesSearch, AzureSearchGamesSearch>();
}
else
{
    builder.Services.AddScoped<IGamesSearch, NoopGamesSearch>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
    db.Database.EnsureCreated();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("ok"));

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class GamesDbContext : DbContext
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameEvent> GameEvents => Set<GameEvent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(b =>
        {
            b.ToTable("Games");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.TagsJson);
        });
        modelBuilder.Entity<GameEvent>(b =>
        {
            b.ToTable("GameEvents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
        });
    }
}

public class Game
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Genre { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string TagsJson { get; set; } = "[]";
}

public class GameEvent
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
