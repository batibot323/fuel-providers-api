using Amerigas.FuelProviders.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Providers
{
    public interface ICosmosDbService
    {
        Task<bool> InsertFuelProviders<T>(IEnumerable<T> collection) where T : BaseEntity;

        Task<dynamic> QueryItems(string query);
    }
}
