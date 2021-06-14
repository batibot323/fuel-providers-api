﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string _key = @"5SGA8ft28dINUNGCwccpccI7FE1TvLVovlOPk1U5V3th71xTNYLR1E0SvwwbkpJCNF3x2vR6DVumy1jrWM8tIg==";
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
                result = "Error: " + ex.Message;
            }

            return result;
        }

        public async Task<bool> BulkInsert(object n)
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

                var query = "SELECT c._self FROM c";
                var pKey = "GASUL";
                var result = await DeleteAll(spId, query, pKey);
            }
            catch (Exception e)
            {

                throw;
            }
            return false;
        }

        public async Task<bool> DeleteAll(string spId, string query, string partitionKey)
        {
            try
            {
                var result = await _client.GetContainer(_databaseName, _containerName).Scripts.ExecuteStoredProcedureAsync<dynamic>(spId, new PartitionKey(partitionKey), new[] { query });
                if (result.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            catch (Exception e)
            {

                throw;
            }
            return false;
        }

        public async Task<bool> CreateStoredProcedure(string storedProcedureId)
        {
            StoredProcedureResponse storedProcedureResponse = await _client.GetContainer(_databaseName, _containerName).Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties
            {
                Id = storedProcedureId,
                Body = File.ReadAllText($@".\CosmosScripts\{storedProcedureId}.js")
            });

            return storedProcedureResponse.StatusCode == HttpStatusCode.Created ? true : false;
        }

    }
}
