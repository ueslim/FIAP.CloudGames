var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Aqui registrar os serviços (ex: DbContext, Repositories, etc)
// builder.Services.AddDbContext<SeuDbContext>(...);
// builder.Services.AddScoped<ISeuServico, SeuServico>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de tratamento global de erros (pode implementar depois)
// app.UseMiddleware<SeuMiddlewareDeErro>();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok("API funcionando!"));

app.Run();
