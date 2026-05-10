using ArkWatch.Server.Data;
using ArkWatch.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. ADD SERVICES
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ArkWatchDbContext>(options =>
    options.UseSqlite("Data Source=ArkWatch.db"));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<WeatherWatcher>();

// 2. DEFINE THE DOOR POLICY
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// 3. CONFIGURE THE CONVEYOR BELT (Order Matters!)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// UseCors MUST come after UseRouting (implicit) and before MapControllers
app.UseRouting();
app.UseCors("AllowAngular");

app.UseAuthorization();

// This is what handles your /api/news calls
app.MapControllers();

// Handle the Angular files
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("/index.html");

app.Run();