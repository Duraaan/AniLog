using AniLog.API.Data;
using AniLog.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Base de datos PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient para Jikan API
builder.Services.AddHttpClient<JikanService>(client =>
{
    client.BaseAddress = new Uri("https://api.jikan.moe/v4/");
});

// CORS para el frontend React (puerto de Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// El orden importa: CORS antes que los controllers
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
