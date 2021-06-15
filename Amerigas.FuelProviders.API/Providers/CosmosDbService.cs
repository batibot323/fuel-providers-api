using Amerigas.FuelProviders.API.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amerigas.FuelProviders.API.Providers
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly ILogger<CosmosDbService> _logger;

        private readonly string _databaseName;
        private readonly string _containerName;
        private readonly string _account;
        private readonly string _key;
        private readonly string _partitionKey;
        private readonly CosmosClient _client;
        private Container _container;

        public CosmosDbService(IConfiguration config, ILogger<CosmosDbService> logger)
        {
            _logger = logger;

            _databaseName = config.GetSection("CosmosDb")["DatabaseName"];
            _containerName = config.GetSection("CosmosDb")["ContainerName"];
            _account = config.GetSection("CosmosDb")["Account"];
            _key = config.GetSection("CosmosDb")["CosmosDBKey"];
            _partitionKey = config.GetSection("CosmosDb")["PartitionKey"];

            CosmosClientOptions options = new CosmosClientOptions() { AllowBulkExecution = true };
            _client = new CosmosClient(_account, _key, options);
            _container = _client.GetContainer(_databaseName, _containerName);
        }

        public async Task<string> CreateItem(JObject newItem)
        {
            //Add new item in CosmosDB container only if it does not exist yet, check Id property
            string pkey = newItem["FuelProvider"].ToString();

            string result;
            try
            {

                ItemResponse<JObject> itemResponse = await _container.ReadItemAsync<JObject>(newItem["id"].ToString(), new PartitionKey(pkey));
                result = "Item with id " + newItem["id"].ToString() + " already exists.";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container if it doesnt exist yet
                ItemResponse<JObject> itemResponse = await _container.CreateItemAsync<JObject>(newItem, new PartitionKey(pkey));
                result = "New item with id " + newItem["id"].ToString() + " created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result = "Error: " + ex.Message;
            }

            return result;
        }

        public async Task<bool> InsertFuelProviders(IEnumerable<FuelProviderRequestModel> fuelProviders)
        {
            // Create Stored procedure if not exists
            try
            {
                string spId = "sp_deleteAll";

                try
                {
                    var spResponse = await _client.GetContainer(_databaseName, _containerName).Scripts.ReadStoredProcedureAsync(spId);
                }
                catch (Exception e)
                {
                    var isCreated = await CreateStoredProcedure(spId);
                }

                // bulk delete
                var query = "SELECT c._self FROM c";
                while (true)
                {
                    var deleteResponse = await DeleteAll(spId, query, _partitionKey);
                    var json = JsonConvert.SerializeObject(deleteResponse, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                    if (json.Contains("\"deleted\": 0"))
                    {
                        // Stop deleting when you there's no more to delete.
                        break;
                    }
                }
                // bulk insert
                List<FuelProvider> transformedFuelProviders = new List<FuelProvider>();
                foreach (var fuelProvider in fuelProviders)
                {
                    var transformedData = new FuelProvider(fuelProvider);
                    transformedFuelProviders.Add(transformedData);
                }
                BulkInsert(transformedFuelProviders, _partitionKey).Wait();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task BulkInsert(IEnumerable<FuelProvider> collection, string partitionKey)
        {
            try
            {
                List<Task> concurrentTasks = new List<Task>();
                foreach (var itemToInsert in collection)
                {
                    itemToInsert.Application ??= partitionKey;

                    if (string.IsNullOrWhiteSpace(itemToInsert.Application))
                        throw new ArgumentNullException("Partition Key is null.");

                    concurrentTasks.Add(_container.CreateItemAsync(itemToInsert, new PartitionKey(itemToInsert.Application)));
                }
                await Task.WhenAll(concurrentTasks);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<StoredProcedureExecuteResponse<dynamic>> DeleteAll(string spId, string query, string partitionKey)
        {
            StoredProcedureExecuteResponse<dynamic> result;
            try
            {
                result = await _client.GetContainer(_databaseName, _containerName).Scripts.ExecuteStoredProcedureAsync<dynamic>(spId, new PartitionKey(partitionKey), new[] { query });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }

            return result;
        }

        public async Task<bool> CreateStoredProcedure(string storedProcedureId)
        {
            try
            {
                StoredProcedureResponse storedProcedureResponse = await _client.GetContainer(_databaseName, _containerName).Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties
                {
                    Id = storedProcedureId,
                    Body = File.ReadAllText($@".\CosmosScripts\{storedProcedureId}.js")
                });

                return storedProcedureResponse.StatusCode == HttpStatusCode.Created ? true : false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<dynamic> QueryItems(string query, string continuationToken = null)
        {
            try
            {

                string decodedToken = null;
                string validToken = null;

                // Decode the continuationToken
                 if (!string.IsNullOrWhiteSpace(continuationToken))
                {
                    var bytes = System.Convert.FromBase64String(continuationToken);
                    decodedToken = Encoding.UTF8.GetString(bytes);
                }

                QueryDefinition queryDefinition = new QueryDefinition(query);
                var responseList = new List<dynamic>();

                using (var feedIterator = _container.GetItemQueryIterator<dynamic>(
                    queryDefinition,
                    decodedToken,
                    new QueryRequestOptions() { PartitionKey = new PartitionKey(_partitionKey), MaxItemCount = 10 }))
                {

                    var response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        responseList.Add(item);
                    }

                    continuationToken = response.ContinuationToken;

                    // Encode the continuationToken
                    if (!string.IsNullOrWhiteSpace(continuationToken))
                    {
                        var encodedToken = Encoding.UTF8.GetBytes(continuationToken);
                        validToken = System.Convert.ToBase64String(encodedToken);
                    }
                }
                return  new { Data = responseList, ContinuationToken = validToken };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }
    }
}
