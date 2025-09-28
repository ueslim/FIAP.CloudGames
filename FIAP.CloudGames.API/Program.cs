using FIAP.CloudGames.API.Configurations;
using FIAP.CloudGames.API.Middlewares;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiConfiguration()                   // Api Configurations
       .AddDatabaseConfiguration()              // Setting DBContexts
       .AddSwaggerConfiguration()               // Swagger Config
       .AddDependencyInjectionConfiguration();  // DI

var app = builder.Build();


//Middleware do Prometheus para coletar métricas de requisições HTTP
app.UseHttpMetrics();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

//Middleware que cria o endpoint /metrics para o Prometheus ler os dados
app.UseMetricServer();


//app.MapGet("/health", () => Results.Ok("API funcionando!"));

app.UseSwaggerSetup();

// Applying migrations and seeding some data
app.UseDbSeed();

app.Run();