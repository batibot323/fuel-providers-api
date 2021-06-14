using Microsoft.Azure.Cosmos.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Models
{
    public class FuelProvider : BaseEntity
    {
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public Point Location { get; set; }
        public string Brand { get; set; }

        public FuelProvider(FuelProviderRequestModel request)
        {
            StoreName = request?.StoreName;
            Address = request?.Address;
            City = request?.City;
            State = request?.State;
            ZipCode = request?.ZipCode;
            //Location = request != null ? new GeographyPoint(request.LATITUDE, request.LONGITUDE) : null;

            Location = request != null ? new Point(request.LONGITUDE, request.LATITUDE) : null;
            Brand = request?.Brand;
        }
    }
}
