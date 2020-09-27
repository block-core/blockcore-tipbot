using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TipBot.Database;

namespace Blockcore.TipBot.Dashboard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        protected readonly IServiceProvider services;

        private readonly IContextFactory contextFactory;

        public WeatherForecastController(
            IServiceProvider services,
            IContextFactory contextFactory)
        {
            this.services = services;
            this.contextFactory = contextFactory;
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                // Figure out how to do sorting on the database, issues with syntax.
                return Ok(context.TipsHistory.ToList().OrderByDescending(t => t.CreationTime));
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (BotDbContext context = this.contextFactory.CreateContext())
            {
                return Ok(context.Users.ToList());
            }
        }
    }
}
