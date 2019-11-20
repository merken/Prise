using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbPlugin
{
    public abstract class MongoDbRepositoryBase<T>
        where T : IHaveAnId
    {
        protected readonly IMongoClient client;
        protected readonly IMongoDatabase database;
        protected readonly string collection;

        protected MongoDbRepositoryBase(MongoDbConfig config, string collection)
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(config.ConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            this.client = new MongoClient(settings);
            this.database = client.GetDatabase(config.Database);
            this.collection = collection;
        }

        protected IMongoCollection<T> Collection => this.database.GetCollection<T>(this.collection);

        protected async Task<IEnumerable<T>> GetAll()
        {
            var results = await this.Collection.Find(_ => true).ToListAsync();
            return results;
        }

        protected async Task<IEnumerable<T>> Search(Expression<Func<T, bool>> predicate = null)
        {
            var results = await this.Collection.Find(predicate).ToListAsync();
            return results;
        }

        protected async Task<T> Insert(T document)
        {
            await this.Collection.InsertOneAsync(document);
            return document;
        }

        protected async Task<T> Update(string documentId, T document)
        {
            var filter = Builders<T>.Filter.Eq(d => d.Id, documentId);
            await this.Collection.ReplaceOneAsync(filter, document);
            return document;
        }

        protected async Task<T> GetItem(string documentId)
        {
            var document = await this.Collection
                            .Find(d => d.Id == documentId)
                            .FirstOrDefaultAsync();

            return document;
        }

        protected async Task Delete(string documentId)
        {
            var filter = Builders<T>.Filter.Eq(d => d.Id, documentId);
            await this.Collection.DeleteOneAsync(filter);
        }
    }
}
