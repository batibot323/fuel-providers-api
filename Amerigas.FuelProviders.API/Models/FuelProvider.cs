using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Models
{
    public class FuelProvider
    {
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public GeographyPoint Location { get; set; }
        public double LATITUDE { get; set; }
        public double LONGITUDE { get; set; }
        public string Brand { get; set; }
    }

    //public ClassNamePlaceholder()
    //{

    //}
}
