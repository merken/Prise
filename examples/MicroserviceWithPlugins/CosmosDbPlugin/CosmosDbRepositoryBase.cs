using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    public abstract class CosmosDbRepositoryBase
    {
        private Container container;
        private readonly CosmosDbConfig config;

        public CosmosDbRepositoryBase(CosmosDbConfig config)
        {
            this.config = config;
        }

        protected async Task InitializeAsync()
        {
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(config.Account, config.Key);
            CosmosClient client = clientBuilder
                                .WithConnectionModeDirect()
                                .Build();
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(config.Database);
            await database.Database.CreateContainerIfNotExistsAsync(config.Container, "/id");
            this.container = client.GetContainer(config.Database, config.Container);
        }

        public async Task AddItemAsync(ProductDocument item)
        {
            await this.container.CreateItemAsync<ProductDocument>(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this.container.DeleteItemAsync<ProductDocument>(id, new PartitionKey(id));
        }

        public async Task<ProductDocument> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<ProductDocument> response = await this.container.ReadItemAsync<ProductDocument>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

        }

        public async Task<IEnumerable<ProductDocument>> GetItemsAsync(string queryString = "")
        {
            var query = this.container.GetItemQueryIterator<ProductDocument>(new QueryDefinition(queryString));
            List<ProductDocument> results = new List<ProductDocument>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, ProductDocument item)
        {
            await this.container.UpsertItemAsync<ProductDocument>(item, new PartitionKey(id));
        }
    }
}
