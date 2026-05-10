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
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Watcher is starting up...");

            try
            {
                // 1. OPEN THE TOOLBOX
                using (var scope = _scopeFactory.CreateScope())
                {
                    var database = scope.ServiceProvider.GetRequiredService<ArkWatchDbContext>();
                    var client = _httpClientFactory.CreateClient();

                    // 2. CHECK THE SHELVES
                    int count = database.StoredAlerts.Count();
                    _logger.LogInformation("I found {count} alerts in the database.", count);

                    if (count == 0)
                    {
                        _logger.LogWarning("Database is empty! Adding the Test Alert now...");
                        var testAlert = new Alert
                        {
                            SourceId = "TEST-123",
                            Headline = "TORNADO WATCH: HEBER SPRINGS AREA",
                            UrgencyLevel = "Extreme",
                            DetailedInstructions = "This is a test. Seek shelter in a sturdy building.",
                            SystemTimestamp = DateTime.UtcNow
                        };
                        database.StoredAlerts.Add(testAlert);
                        await database.SaveChangesAsync(stoppingToken);
                    }

                    // 3. GET REAL DATA
                    client.DefaultRequestHeaders.Add("User-Agent", "(ArkWatchProject, your@email.com)");

                    var response = await client.GetFromJsonAsync<NwsResponse>(
                        "https://api.weather.gov/alerts/active?area=AR", stoppingToken);

                    if (response?.Features != null)
                    {
                        foreach (var feature in response.Features)
                        {
                            var alertData = feature.Properties;

                            if (alertData.Severity == "Extreme" || alertData.Severity == "Severe")
                            {
                                var newAlert = new Alert
                                {
                                    SourceId = alertData.Id,
                                    Headline = alertData.Event,
                                    UrgencyLevel = alertData.Severity,
                                    DetailedInstructions = alertData.Description,
                                    SystemTimestamp = DateTime.UtcNow
                                };
                                database.StoredAlerts.Add(newAlert);
                            }
                        }

                        // LOCK IN THE SAVES
                        await database.SaveChangesAsync(stoppingToken);
                    }
                } // <--- The "Toolbox" closes here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Watcher hit a snag.");
            }

            // Wait 1 minute before checking again
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}