using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Security.Claims;
using System.Text;
using Azure.Storage.Queues;
using System.Text.Json;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// CORS for Angular localhost and Azure Static Web Apps
var allowedOrigins = new[] { "http://localhost:4200", "https://*.azurestaticapps.net" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// EF Core DbContext
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("UsersDb")));

// Queue publisher for user events
builder.Services.AddSingleton(sp =>
{
    var cs = configuration.GetConnectionString("Storage") ?? "UseDevelopmentStorage=true";
    return new QueueClient(cs, configuration["Queues:Users"] ?? "users-events");
});

// JWT Authentication (issuer in Users service)
var jwtKey = configuration["Jwt:Secret"] ?? "replace-me";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "Fiap.CloudGames.Users";
var jwtAudience = configuration["Jwt:Audience"] ?? "Fiap.CloudGames.Clients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
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

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CloudGames Users API", Version = "v1" });
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

// OpenTelemetry (App Insights may still collect via ILogger)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Fiap.CloudGames.Users"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation());

// DI for domain/app services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
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

// Hexagonal: Domain + Ports & Adapters kept in same project via folders for simplicity
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Email).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.IsActive).HasDefaultValue(true);
        });
        modelBuilder.Entity<UserEvent>(b =>
        {
            b.ToTable("UserEvents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
        });
    }
}

public enum UserRole { User = 0, Administrator = 1 }

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class UserEvent
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record CreateUserDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
public record UserDto(Guid Id, string Name, string Email, string Role);
public record LoginResponseDto(string Token, UserDto User);

public interface ITokenService { string GenerateToken(User user); }
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration) { _configuration = configuration; }
    public string GenerateToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "replace-me");
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}

public interface IUserService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto> RegisterAsync(CreateUserDto dto);
    Task<UserDto> GetMeAsync(Guid userId);
}

public class UserService : IUserService
{
    private readonly UsersDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly QueueClient _queue;
    public UserService(UsersDbContext db, ITokenService tokenService, QueueClient queue)
    {
        _db = db; _tokenService = tokenService; _queue = queue;
    }
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
        var token = _tokenService.GenerateToken(user);
        return new LoginResponseDto(token, new UserDto(user.Id, user.Name, user.Email, user.Role.ToString()));
    }
    public async Task<UserDto> RegisterAsync(CreateUserDto dto)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists) throw new InvalidOperationException("Email already in use");
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User,
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var evt = new { Type = "UserRegistered", user.Id, user.Name, user.Email };
        _db.UserEvents.Add(new UserEvent { Id = Guid.NewGuid(), Type = "UserRegistered", Payload = JsonSerializer.Serialize(evt), CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        await _queue.CreateIfNotExistsAsync();
        await _queue.SendMessageAsync(JsonSerializer.Serialize(evt));
        return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString());
    }
    public async Task<UserDto> GetMeAsync(Guid userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (user == null) throw new KeyNotFoundException("User not found");
        return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString());
    }
}
