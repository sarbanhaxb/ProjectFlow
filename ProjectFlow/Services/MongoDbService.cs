
using MongoDB.Bson;
using MongoDB.Driver;

namespace ProjectFlow.Services;

public sealed class MongoDbService
{
    // Единый экземпляр сервиса для приложения
    public static MongoDbService Instance { get; } = new();

    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;

    // Запрещаем создание экземпляров извне
    private MongoDbService()
    {
        _client = new MongoClient(
            "mongodb://localhost:27017");

        _database = _client.GetDatabase("ProjectFlowDB");
    }

    public IMongoDatabase Database => _database;

    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var command = new BsonDocument("ping", 1);

            await _database.RunCommandAsync<BsonDocument>(
                command);

            return true;
        }
        catch (MongoException)
        {
            return false;
        }
    }
}
