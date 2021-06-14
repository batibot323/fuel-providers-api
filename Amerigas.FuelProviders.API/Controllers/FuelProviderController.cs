using Amerigas.FuelProviders.API.Models;
using Amerigas.FuelProviders.API.Providers;
using Microsoft.AspNetCore.Cors;
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
    [Route("fuelProviders")]
    public class FuelProviderController : ControllerBase
    {
        private ICosmosDbService _cosmosDbService;

        private readonly ILogger<FuelProviderController> _logger;

        public FuelProviderController(ILogger<FuelProviderController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        [HttpPost]
        [EnableCors("AllowAll")]
        public async Task<IActionResult> InsertFuelProviders([FromBody]FuelProviderRequestModel[] fuelProviders)
        {
            //var myObject = new
            //{
            //    id = Guid.NewGuid(),
            //    FuelProvider = "GASUL",
            //    Name = "Hani",
            //    Level = 85,
            //    Hobby = "Cooking",
            //    FavoriteFood = "Lasagna",
            //    FavoriteDay = "Monday",
            //    Color = "Orange"
            //};
            //JObject o = (JObject)JToken.FromObject(myObject);
            //return Ok(await _cosmosDbService.CreateItem(o));

            var result = await _cosmosDbService.InsertFuelProviders(fuelProviders);
            return Ok();
        }
    }
}
