using ArkWatch.Server.Data;
using ArkWatch.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArkWatch.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly ArkWatchDbContext _context;

        public NewsController(ArkWatchDbContext context)
        {
            _context = context;
        }

        [HttpGet("headlines")]
        public async Task<IActionResult> GetHeadlines()
        {
            var alerts = await _context.StoredAlerts
                .OrderByDescending(a => a.SystemTimestamp)
                .Take(20)
                .ToListAsync();

            var formattedHeadlines = alerts.Select(a => new {
                a.SourceId,
                a.Headline,
                a.DetailedInstructions,
                a.UrgencyLevel,
                a.SystemTimestamp,
                Expiration = a.Expiration ?? "N/A",
                HailSize = a.HailSize ?? "0.00",
                WindSpeed = a.WindSpeed ?? "0",
                Category = GetCategory(a.UrgencyLevel)
            });

            return Ok(formattedHeadlines);
        }

        [HttpGet("ticker")]
        public async Task<IActionResult> GetTickerHeadlines()
        {
            var alerts = await _context.StoredAlerts
                .OrderByDescending(a => a.SystemTimestamp)
                .Take(10)
                .ToListAsync();

            var tickerLines = alerts.Select(a => new {
                Text = a.Headline,
                Category = GetCategory(a.UrgencyLevel)
            }).ToList();

            // If empty, add a default info line
            if (!tickerLines.Any())
            {
                tickerLines.Add(new { Text = "ARKWATCH: NO ACTIVE THREATS DETECTED", Category = "info" });
            }

            return Ok(tickerLines);
        }

        // Helper method to keep logic consistent across both endpoints
        private static string GetCategory(string urgency)
        {
            return (urgency?.ToLower()) switch
            {
                "extreme" => "warning",
                "severe" => "watch",
                _ => "info"
            };
        }
    }
}