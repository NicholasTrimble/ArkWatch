using ArkWatch.Server.Data;
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
            var headlines = await _context.StoredAlerts
                .OrderByDescending(a => a.SystemTimestamp)
                .Take(20)
                .ToListAsync();
            return Ok(headlines);
        }
    }
}
