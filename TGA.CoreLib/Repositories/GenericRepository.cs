using MongoDB.Driver;
using System.Linq.Expressions;
using TGA.CoreLib.Repositories.Interfaces;

namespace TGA.CoreLib.Repositories
{
    public class GenericRepository : IGenericRepository
    {
        private readonly IMongoDatabase mongoDatabase;
        private readonly ReplaceOptions replaceOptions = new() { IsUpsert = true };
        public GenericRepository(IMongoDatabase mongoDatabase)
        {
            this.mongoDatabase = mongoDatabase;
        }

        public async Task LogData<TData>(TData data, Expression<Func<TData, bool>>? filter = null)
        {
            IMongoCollection<TData> collection = mongoDatabase.GetCollection<TData>(GetCollectionName<TData>());
            if (filter == null)
            {
                await collection.InsertOneAsync(data);
            }
            else
            {
                await collection.ReplaceOneAsync(filter, data, replaceOptions);
            }
        }

        public async Task<List<TData>> GetData<TData>(Expression<Func<TData, bool>> filter)
        {
            IMongoCollection<TData> collection = mongoDatabase.GetCollection<TData>(GetCollectionName<TData>());
            IAsyncCursor<TData> result = await collection.FindAsync(Builders<TData>.Filter.Where(filter));
            return await result.ToListAsync();
        }

        public static string GetCollectionName<T>()
        {
            return typeof(T).Name + "s";
        }
    }
}
