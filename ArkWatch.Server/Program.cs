using ArkWatch.Server.Data;
using Microsoft.EntityFrameworkCore;
using ArkWatch.Server.Data;
using ArkWatch.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ArkWatchDbContext>(options =>
    options.UseSqlite("Data Source=ArkWatch.db"));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<WeatherWatcher>();
var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
