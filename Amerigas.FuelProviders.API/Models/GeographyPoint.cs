using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Models
{
    public class GeographyPoint
    {
        public string Type { get; set; }
        public double[] Coordinates { get; set; }

        public GeographyPoint(double latitude, double longitude)
        {
            Type = "Point";
            double[] coordinates = new [] { latitude, longitude };
        }
    }
}
