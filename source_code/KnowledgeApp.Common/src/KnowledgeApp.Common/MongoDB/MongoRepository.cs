using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace KnowledgeApp.Common.MongoDB
{

    public class MongoRepository<T> : IRepository<T> where T : IModel
    {
        private readonly IMongoCollection<T> dbCollection;
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            dbCollection = database.GetCollection<T>(collectionName);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).ToListAsync();
        }        

        public async Task<T> GetAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.Id, id);
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }        

        public async Task CreateAsync(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            await dbCollection.InsertOneAsync(model);
        }

        public async Task UpdateAsync(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            FilterDefinition<T> filter = filterBuilder.Eq(existingModel => existingModel.Id, model.Id);
            await dbCollection.ReplaceOneAsync(filter, model);
        }

        public async Task RemoveAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(model => model.Id, id);
            await dbCollection.DeleteOneAsync(filter);
        }
    }
}