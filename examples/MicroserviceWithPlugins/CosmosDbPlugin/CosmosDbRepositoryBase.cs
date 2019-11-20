using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    public abstract class CosmosDbRepositoryBase<T>
         where T : class
    {
        private DocumentClient client;
        private readonly CosmosDbConfig config;

        public CosmosDbRepositoryBase(CosmosDbConfig config)
        {
            this.config = config;
        }

        protected async Task InitializeAsync()
        {
            this.client = new DocumentClient(new Uri(config.Account), config.Key);
            await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(config.Database));
            await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(config.Database, config.Container));
        }

        public async Task<Document> AddItemAsync(T item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(config.Database, config.Container), item);
        }

        public async Task DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(config.Database, config.Container, id));
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                var document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(config.Database, config.Container, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate = null)
        {
            var query = (IQueryable<T>)client.CreateDocumentQuery<T>(
                 UriFactory.CreateDocumentCollectionUri(config.Database, config.Container),
                 new FeedOptions { MaxItemCount = -1 });

            if (predicate != null)
                query = query
                 .Where(predicate);

            var documentQuery = query.AsDocumentQuery();
            List<T> results = new List<T>();
            while (documentQuery.HasMoreResults)
            {
                results.AddRange(await documentQuery.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(config.Database, config.Container, id), item);
        }
    }
}
