using ArkWatch.Server.Data;
using ArkWatch.Server.Models;
using static ArkWatch.Server.Models.Alert;

namespace ArkWatch.Server.Services;

public class WeatherWatcher : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeatherWatcher> _logger;

    public WeatherWatcher(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<WeatherWatcher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // This loop keeps running until you stop the app
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Watcher is checking for Arkansas weather alerts...");

            try
            {
                // We have to create a "Scope" to talk to the database from a background worker
                using (var scope = _scopeFactory.CreateScope())
                {
                    var database = scope.ServiceProvider.GetRequiredService<ArkWatchDbContext>();
                    var client = _httpClientFactory.CreateClient();

                    // The Weather Service requires a 'User-Agent' so they know who is asking
                    client.DefaultRequestHeaders.Add("User-Agent", "(ArkWatchDevProject, your@email.com)");

                    // Get the latest active alerts for Arkansas
                    var response = await client.GetFromJsonAsync<NwsResponse>(
                        "https://api.weather.gov/alerts/active?area=AR", stoppingToken);

                    if (response?.Features != null)
                    {
                        foreach (var feature in response.Features)
                        {
                            var alertData = feature.Properties;

                            // Check if we ALREADY have this alert saved in our cabinet
                            bool alreadyExists = database.StoredAlerts.Any(a => a.SourceId == alertData.Id);

                            if (!alreadyExists)
                            {
                                // Create a new entry for our database
                                var newAlert = new Alert
                                {
                                    SourceId = alertData.Id,
                                    Headline = alertData.Event,
                                    UrgencyLevel = alertData.Severity,
                                    DetailedInstructions = alertData.Description,
                                    SystemTimestamp = DateTime.UtcNow
                                };

                                database.StoredAlerts.Add(newAlert);
                                _logger.LogWarning("NEW ALERT SAVED: {headline}", newAlert.Headline);
                            }
                        }

                        // Actually "Lock in" the changes to the database
                        await database.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Watcher hit a snag while checking the weather.");
            }

            // Wait 1 minute before checking again
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}