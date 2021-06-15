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
            try
            {
                var isSuccess = await _cosmosDbService.InsertFuelProviders(fuelProviders);
                if(isSuccess == true)
                    return Ok("Bulk Insert Successful.");
                
                return StatusCode(500);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> AreaRadialSearch([FromBody]dynamic request, [FromQuery]string continuationToken = null)
        {
            try
            {
                if (request["location"] == null)
                    return StatusCode(400, "Invalid request. Missing location field.");

                if (request["radiusKilometers"] == null)
                    return StatusCode(400, "Invalid request. Missing radius field.");

                var location = ((JObject)request["location"]).ToString();
                var radius = (Convert.ToInt32(request["radiusKilometers"])) * 1000;

                string query = $"SELECT * FROM c WHERE  ST_DISTANCE(c.Location, {location}) < {radius}";

                var result= await _cosmosDbService.QueryItems(query, continuationToken);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
