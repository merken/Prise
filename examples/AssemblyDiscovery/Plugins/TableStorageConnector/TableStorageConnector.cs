using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableStorageConnector
{
    public class TableStorageConnector<T>
        where T : class, new()
    {
        private readonly TableStorageConfig config;
        private readonly Func<T, string> partitionKeyGetter;
        private readonly Action<T, string> partitionKeySetter;
        private readonly Func<T, string> rowKeyGetter;
        private readonly Action<T, string> rowKeySetter;
        private CloudTable table;

        protected TableStorageConnector(
            TableStorageConfig config,
            Func<T, string> partitionKeyGetter,
            Action<T, string> partitionKeySetter,
            Func<T, string> rowKeyGetter,
            Action<T, string> rowKeySetter)
        {
            this.config = config;
            this.partitionKeyGetter = partitionKeyGetter;
            this.partitionKeySetter = partitionKeySetter;
            this.rowKeyGetter = rowKeyGetter;
            this.rowKeySetter = rowKeySetter;
        }

        private async Task ConnectToTableAsync()
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(this.config.StorageAccount, this.config.StorageKey), false);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            this.table = tableClient.GetTableReference(this.config.TableName);
            await table.CreateIfNotExistsAsync();
        }

        protected async Task<IEnumerable<T>> GetAll()
        {
            await ConnectToTableAsync();
            TableQuery<EntityAdapter<T>> query = new TableQuery<EntityAdapter<T>>();

            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<EntityAdapter<T>> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results.Select(e => e.Value));

            } while (continuationToken != null);

            return results;
        }

        protected async Task<IEnumerable<T>> Search(string term)
        {
            await ConnectToTableAsync();
            TableQuery<EntityAdapter<T>> query = new TableQuery<EntityAdapter<T>>();
            query.FilterString = term;

            List<T> results = new List<T>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<EntityAdapter<T>> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                results.AddRange(queryResults.Results.Select(e => e.Value));

            } while (continuationToken != null);

            return results;
        }

        protected async Task<T> InsertOrUpdate(T entity)
        {
            await ConnectToTableAsync();
            var operation = TableOperation.InsertOrReplace(AsAdapter(entity));
            await this.table.ExecuteAsync(operation);
            return entity;
        }

        protected async Task<T> GetItem(string partitionKey, string rowKey)
        {
            await ConnectToTableAsync();
            var adapter = AsAdapter(partitionKey, rowKey);
            var operation = TableOperation.Retrieve<EntityAdapter<T>>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            return (T)(dynamic)result.Result;
        }

        protected async Task Delete(string partitionKey, string rowKey)
        {
            await ConnectToTableAsync();
            var item = await GetItem(partitionKey, rowKey);
            var adapter = AsAdapter(item);
            var operation = TableOperation.Delete(adapter);
            await this.table.ExecuteAsync(operation);
        }

        private EntityAdapter<T> AsAdapter(string partitionKey, string rowKey) => new EntityAdapter<T>(partitionKey, rowKey);
        private EntityAdapter<T> AsAdapter(T entity) => new EntityAdapter<T>(entity, this.partitionKeyGetter, this.partitionKeySetter, this.rowKeyGetter, this.rowKeySetter);
    }
}
