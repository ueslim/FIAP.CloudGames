using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins("http://localhost:4200", "https://*.azurestaticapps.net")
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddDbContext<PaymentsDbContext>(opt =>
    opt.UseSqlServer(config.GetConnectionString("PaymentsDb")));

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
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("Fiap.CloudGames.Payments"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddEntityFrameworkCoreInstrumentation().AddAzureMonitorTraceExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation().AddAzureMonitorMetricExporter());

builder.Services.AddSingleton(sp =>
{
    var cs = config.GetConnectionString("Storage") ?? "UseDevelopmentStorage=true";
    return new QueueClient(cs, config["Queues:Payments"] ?? "payments-events");
});

builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("ok"));

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.GameId).IsRequired();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Status).HasMaxLength(30);
        });
        modelBuilder.Entity<PaymentEvent>(b =>
        {
            b.ToTable("PaymentEvents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
        });
    }
}

public enum PaymentStatus { Pending, Succeeded, Failed }

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PaymentEvent
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record InitiatePaymentDto(Guid GameId, decimal Amount);
public record PaymentResponseDto(Guid PaymentId, string Status);

public interface IPaymentService
{
    Task<PaymentResponseDto> InitiateAsync(Guid userId, InitiatePaymentDto dto);
    Task<PaymentResponseDto> GetStatusAsync(Guid id);
}

public class PaymentService : IPaymentService
{
    private readonly PaymentsDbContext _db;
    private readonly QueueClient _queue;
    public PaymentService(PaymentsDbContext db, QueueClient queue) { _db = db; _queue = queue; }
    public async Task<PaymentResponseDto> InitiateAsync(Guid userId, InitiatePaymentDto dto)
    {
        var payment = new Payment { UserId = userId, GameId = dto.GameId, Amount = dto.Amount, Status = PaymentStatus.Pending };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        var evt = new { Type = "GamePurchased", PaymentId = payment.Id, payment.UserId, payment.GameId, payment.Amount };
        _db.PaymentEvents.Add(new PaymentEvent { Id = Guid.NewGuid(), Type = "GamePurchased", Payload = JsonSerializer.Serialize(evt), CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        await _queue.CreateIfNotExistsAsync();
        await _queue.SendMessageAsync(JsonSerializer.Serialize(evt));
        return new PaymentResponseDto(payment.Id, payment.Status.ToString());
    }
    public async Task<PaymentResponseDto> GetStatusAsync(Guid id)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p == null) throw new KeyNotFoundException("Payment not found");
        return new PaymentResponseDto(p.Id, p.Status.ToString());
    }
}
