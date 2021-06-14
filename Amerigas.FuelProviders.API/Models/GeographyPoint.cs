using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Models
{
    public class GeographyPoint
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public double?[] Coordinates { get; set; }

        public GeographyPoint(double? latitude, double? longitude)
        {
            Type = "Point";
            Coordinates = new [] { latitude, longitude };
        }
    }
}
