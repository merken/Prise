using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    public abstract class MongoDbRepositoryBase<T>
    {
        protected readonly IMongoClient client;
        protected readonly IMongoDatabase database;
        protected readonly string collection;

        protected MongoDbRepositoryBase(MongoDbConfig config, string collection)
        {
            this.client = new MongoClient(config.ConnectionString);
            this.database = client.GetDatabase(config.Database);
            this.collection = collection;
        }

        protected IMongoCollection<DocumentBase> Collection => this.database.GetCollection<DocumentBase>(this.collection);

        protected async Task<IEnumerable<T>> GetAll()
        {
            var results = await this.Collection.Find(_ => true).ToListAsync();
            return results.Select(d => ConvertBody(d));
        }

        protected async Task<IEnumerable<T>> Search(string term)
        {
            var results = await this.Collection.Find(_ => _.Body.Contains(term)).ToListAsync();
            return results.Select(d => ConvertBody(d));
        }

        protected async Task<T> Insert(T document)
        {
            await this.Collection.InsertOneAsync(new DocumentBase() { Body = System.Text.Json.JsonSerializer.Serialize(document) });
            return document;
        }

        protected async Task<T> Update(string documentId, T document)
        {
            var filter = Builders<DocumentBase>.Filter.Eq(d => d.Id, documentId);
            var update = Builders<DocumentBase>.Update.Set(d => d.Body, System.Text.Json.JsonSerializer.Serialize(document));
            await this.Collection.UpdateOneAsync(filter, update);
            return document;
        }

        protected async Task<T> GetItem(string documentId)
        {
            var document =  await this.Collection
                            .Find(d => d.Id == documentId)
                            .FirstOrDefaultAsync();

            return ConvertBody(document);
        }

        protected async Task Delete(string documentId)
        {
            await this.Collection.DeleteOneAsync(
                   Builders<DocumentBase>.Filter.Eq("Id", documentId));
        }

        protected T ConvertBody(DocumentBase document)
        {
            return (T)System.Text.Json.JsonSerializer.Deserialize(document.Body, typeof(T));
        }
    }
}
