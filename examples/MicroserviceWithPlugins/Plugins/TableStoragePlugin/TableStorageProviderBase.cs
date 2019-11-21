using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TableStoragePlugin
{
    public abstract class TableStorageProviderBase<T> where T : ProductTableEntity, new()
    {
        private readonly TableStorageConfig config;
        private CloudTable table;

        protected TableStorageProviderBase(TableStorageConfig config)
        {
            this.config = config;
        }

        protected async Task ConnectToTableAsync()
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(this.config.StorageAccount, this.config.StorageKey), false);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            this.table = tableClient.GetTableReference(this.config.TableName);
            await table.CreateIfNotExistsAsync();
        }

        protected async Task<IEnumerable<T>> GetAll()
        {
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
            var operation = TableOperation.InsertOrReplace(entity);
            await this.table.ExecuteAsync(operation);
            return entity;
        }

        protected async Task<T> GetItem(string partitionKey, string rowKey)
        {
            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            return (T)(dynamic)result.Result;
        }

        protected async Task Delete(string partitionKey, string rowKey)
        {
            var item = await GetItem(partitionKey, rowKey);
            var operation = TableOperation.Delete(item);
            await this.table.ExecuteAsync(operation);
        }
    }
}
