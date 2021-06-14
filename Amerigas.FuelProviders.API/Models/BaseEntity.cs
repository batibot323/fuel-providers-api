using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Models
{
    public class BaseEntity
    {
        public string id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("application")]
        public string Application { get; set; }
    }
}
