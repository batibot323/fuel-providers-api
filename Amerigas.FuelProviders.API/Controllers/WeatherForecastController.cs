using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private CosmosDbService _cosmosDbService;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _cosmosDbService = new CosmosDbService();
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("Cosmos")]
        public async Task<string> InsertToCosmos()
        {
            var myObject = new
            {
                id = Guid.NewGuid(),
                Name = "Hani",
                Level = 85,
                Hobby = "Cooking",
                FavoriteFood = "Lasagna",
                FavoriteDay = "Monday",
                Color = "Orange"
            };
            JObject o = (JObject)JToken.FromObject(myObject);
            return await _cosmosDbService.CreateItem(o);
        }
    }
}
