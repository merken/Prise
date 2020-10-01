
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prise.Example.MvcPlugin.AzureTableStorage
{
    public class TableConfig
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
    }

    public abstract class TableStorageProviderBase<T> where T : TableEntity, new()
    {
        private readonly TableConfig config;
        private readonly CloudTable table;

        public TableStorageProviderBase(TableConfig config)
        {
            this.config = config;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.config.ConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            this.table = tableClient.GetTableReference(this.config.TableName);
        }

        protected async Task<IEnumerable<T>> GetAllItems()
        {
            await table.CreateIfNotExistsAsync();

            TableQuery<T> query = new TableQuery<T>();

            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }

        protected async Task<IEnumerable<T>> Search(string term)
        {
            await table.CreateIfNotExistsAsync();

            TableQuery<T> query = new TableQuery<T>();
            query.FilterString = term;

            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<T> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results);

            } while (continuationToken != null);

            return results;
        }

        protected async Task<T> InsertOrUpdate(T entity)
        {
            await table.CreateIfNotExistsAsync();

            var operation = TableOperation.InsertOrReplace(entity);
            await this.table.ExecuteAsync(operation);
            return entity;
        }

        protected async Task<T> GetItem(string partitionKey, string rowKey)
        {
            await table.CreateIfNotExistsAsync();

            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            return (T)(dynamic)result.Result;
        }

        protected async Task Delete(string partitionKey, string rowKey)
        {
            await table.CreateIfNotExistsAsync();

            var item = await GetItem(partitionKey, rowKey);
            var operation = TableOperation.Delete(item);
            await this.table.ExecuteAsync(operation);
        }
    }
}