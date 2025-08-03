using FIAP.CloudGames.API.Configurations;
using FIAP.CloudGames.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiConfiguration()                   // Api Configurations
       .AddDatabaseConfiguration()              // Setting DBContexts
       .AddSwaggerConfiguration()               // Swagger Config
       .AddDependencyInjectionConfiguration();  // DI

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("DevelopmentPolicy");

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//app.MapGet("/health", () => Results.Ok("API funcionando!"));

app.UseSwaggerSetup();

// Applying migrations and seeding some data
app.UseDbSeed();

app.Run();