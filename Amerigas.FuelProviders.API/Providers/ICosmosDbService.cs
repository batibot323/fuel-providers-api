using Amerigas.FuelProviders.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Providers
{
    public interface ICosmosDbService
    {
        Task<bool> InsertFuelProviders(IEnumerable<FuelProviderRequestModel> collection);

        Task<dynamic> QueryItems(string query);
    }
}
