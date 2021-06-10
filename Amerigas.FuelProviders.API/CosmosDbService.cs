using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API
{
    public class CosmosDbService
    {
        private readonly string _databaseName = @"SampleDB";
        private readonly string _containerName = @"Hani-Container";
        private readonly string _account = @"https://amerigascosmostest.documents.azure.com:443/";
        private readonly  string _key = @"5SGA8ft28dINUNGCwccpccI7FE1TvLVovlOPk1U5V3th71xTNYLR1E0SvwwbkpJCNF3x2vR6DVumy1jrWM8tIg==";
        private readonly CosmosClient _client;
        private Container _container;

        public CosmosDbService()
        {
            _client = new CosmosClient(_account, _key);
            _container = _client.GetContainer(_databaseName, _containerName);
        }

        public async Task<string> CreateItem(JObject newItem)
        {
            //Add new item in CosmosDB container only if it does not exist yet, check Id property
            string result;
            try
            {
                ItemResponse<JObject> itemResponse = await _container.ReadItemAsync<JObject>(newItem["id"].ToString(), new PartitionKey(newItem["id"].ToString()));
                result = "Item with id " + newItem["id"].ToString() + " already exists.";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container if it doesnt exist yet
                ItemResponse<JObject> itemResponse = await _container.CreateItemAsync<JObject>(newItem, new PartitionKey(newItem["id"].ToString()));
                result = "New item with id " + newItem["id"].ToString() + " created.";
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.Message;
            }

            return result;
        }

    }
}
