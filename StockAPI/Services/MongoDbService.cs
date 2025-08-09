using MongoDB.Driver;

namespace StockAPI.Services
{
    public class MongoDbService 
    {

        readonly IMongoDatabase _database;
        public MongoDbService(IConfiguration configuration)
        {
            MongoClient client = new(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("SagaStockAPI");
        }

        public IMongoCollection<T> GetCollection<T>() =>
            _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());

    }
}
