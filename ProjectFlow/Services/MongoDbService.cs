using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectFlow.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService()
        {
            var client = new MongoClient("mongodb://localhost:27017");

            _database = client.GetDatabase("ProjectFlowDB");
        }

        public IMongoDatabase Database => _database;

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var command = new BsonDocument("ping", 1);

                await _database.RunCommandAsync<BsonDocument>(command);

                return true;
            }
            catch (MongoException)
            {
                return false;
            }
        }
    }
}
