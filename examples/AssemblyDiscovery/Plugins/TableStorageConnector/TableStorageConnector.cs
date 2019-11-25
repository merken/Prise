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

        protected async Task<IEnumerable<EntityAdapter<T>>> GetAll()
        {
            await ConnectToTableAsync();
            TableQuery<EntityAdapter<T>> query = new TableQuery<EntityAdapter<T>>();

            var results = new List<EntityAdapter<T>>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<EntityAdapter<T>> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                foreach (var entity in queryResults.Results)
                {
                    this.partitionKeySetter(entity.Value, entity.PartitionKey);
                    this.rowKeySetter(entity.Value, entity.RowKey);
                    results.Add(entity);
                }
            } while (continuationToken != null);

            return results;
        }

        protected async Task<IEnumerable<EntityAdapter<T>>> Search(string term)
        {
            await ConnectToTableAsync();
            TableQuery<EntityAdapter<T>> query = new TableQuery<EntityAdapter<T>>();
            query.FilterString = term;

            var results = new List<EntityAdapter<T>>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<EntityAdapter<T>> queryResults =
                    await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = queryResults.ContinuationToken;
                foreach (var entity in queryResults.Results)
                {
                    this.partitionKeySetter(entity.Value, entity.PartitionKey);
                    this.rowKeySetter(entity.Value, entity.RowKey);
                    results.Add(entity);
                }
            } while (continuationToken != null);

            return results;
        }

        protected async Task<EntityAdapter<T>> InsertOrUpdate(T entity)
        {
            await ConnectToTableAsync();
            var operation = TableOperation.InsertOrReplace(AsAdapter(entity));
            var result = await this.table.ExecuteAsync(operation);
            return (EntityAdapter<T>)(dynamic)result.Result;
        }

        protected async Task<EntityAdapter<T>> GetItem(string partitionKey, string rowKey)
        {
            await ConnectToTableAsync();
            var adapter = AsAdapter(partitionKey, rowKey);
            var operation = TableOperation.Retrieve<EntityAdapter<T>>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(operation);

            return (EntityAdapter<T>)(dynamic)result.Result;
        }

        protected async Task Delete(string partitionKey, string rowKey)
        {
            await ConnectToTableAsync();
            var item = await GetItem(partitionKey, rowKey);
            var operation = TableOperation.Delete(item);
            await this.table.ExecuteAsync(operation);
        }

        private EntityAdapter<T> AsAdapter(string partitionKey, string rowKey) => new EntityAdapter<T>(partitionKey, rowKey);
        private EntityAdapter<T> AsAdapter(T entity) => new EntityAdapter<T>(entity, this.partitionKeyGetter, this.partitionKeySetter, this.rowKeyGetter, this.rowKeySetter);
    }
}
