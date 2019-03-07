namespace ToDo.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Models;

    public class CosmosService
    {
        private static CosmosItems items;
        private static CosmosClient client;
        private static string containerId;
        private static string databaseId;
        private static string endpoint;
        private static string primaryKey;
        

        public static async Task<TodoItem> GetTodoItemAsync(string id, string partitionKey)
        {
            TodoItem item = await items.ReadItemAsync<TodoItem>(partitionKey, id);
            return item;
        }

        public static async Task<IEnumerable<TodoItem>> GetOpenItemsAsync()
        {
            var queryText = "SELECT* FROM c WHERE c.isComplete != true";
            var querySpec = new CosmosSqlQueryDefinition(queryText);

            // Selecting all tasks that are not completed is a cross partition query. 
            // We set the max concurrency to 4, which controls the max number of partitions that our client will query in parallel.
            var query = items.CreateItemQuery<TodoItem>(querySpec, 4);

            var results = new List<TodoItem>();
            while (query.HasMoreResults)
            {
                var set = await query.FetchNextSetAsync();
                results.AddRange(set);
            }

            return results;
        }

        public static async Task<TodoItem> CreateItemAsync(TodoItem item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString();
            }

            return await items.CreateItemAsync(item.Category, item);
        }

        public static async Task<TodoItem> UpdateItemAsync(TodoItem item)
        {
            return await items.ReplaceItemAsync(item.Category, item.Id, item);
        }

        public static async Task DeleteItemAsync(string id, string category)
        {
            await items.DeleteItemAsync<TodoItem>(category, id);
        }

        public static async Task Initialize(IConfiguration configuration)
        {
            databaseId = configuration.GetSection("CosmosDb").GetSection("Database").Value ?? "Tasks";
            containerId = configuration.GetSection("CosmosDb").GetSection("Container").Value ?? "Items";
            endpoint = configuration.GetSection("CosmosDb").GetSection("Endpoint").Value;
            primaryKey = configuration.GetSection("CosmosDb").GetSection("PrimaryKey").Value;

            var config = new CosmosConfiguration(endpoint, primaryKey);
            client = new CosmosClient(config);

            CosmosDatabase database = await client.Databases.CreateDatabaseIfNotExistsAsync(databaseId);
            CosmosContainer container =
                await database.Containers.CreateContainerIfNotExistsAsync(containerId, "/category");
            items = container.Items;
        }
    }
}