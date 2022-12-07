using Codebin.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;

namespace Codebin.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Snippet> _snippetCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings, IHttpContextAccessor httpContextAccessor)
        {
            MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_CONNECTION_URI") ?? mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? mongoDBSettings.Value.DatabaseName);
            _snippetCollection = database.GetCollection<Snippet>(Environment.GetEnvironmentVariable("MONGO_COLLECTION") ?? mongoDBSettings.Value.CollectionName);
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Snippet>> GetAsync()
        {
            FilterDefinition<Snippet> filter = Builders<Snippet>.Filter.Eq("user_id", _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            return await _snippetCollection.Find(filter).ToListAsync();
        }

        public async Task CreateAsync(Snippet snippet)
        {
            snippet.UserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _snippetCollection.InsertOneAsync(snippet);
            return;
        }

        public async Task<Snippet> GetOneAsync(string id)
        {
            FilterDefinition<Snippet> filter = Builders<Snippet>.Filter.Eq("Id", id)
                & Builders<Snippet>.Filter.Eq("user_id", _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            return await _snippetCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Snippet snippet)
        {
            try
            {
                FilterDefinition<Snippet> filter = Builders<Snippet>.Filter.Eq(c => c.Id, snippet.Id)
                    & Builders<Snippet>.Filter.Eq("user_id", _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                UpdateDefinition<Snippet> update = Builders<Snippet>.Update.Set("tags", snippet.Tags)
                    .Set("content", snippet.Content)
                    .Set("title", snippet.Title)
                    .Set("language", snippet.Language);

                await _snippetCollection.UpdateOneAsync(filter, update);
                return;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteAsync(string Id)
        {
            try
            {
                FilterDefinition<Snippet> filter = Builders<Snippet>.Filter.Eq(c => c.Id, Id)
                    & Builders<Snippet>.Filter.Eq("user_id", _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _snippetCollection.DeleteOneAsync(filter);
                return;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Snippet>> FindAsync(string query)
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$search", new BsonDocument {
                    {
                        "index",
                        "default"
                    },
                    {
                        "text",
                        new BsonDocument {
                        {
                            "query",
                            query
                        },
                        {
                            "path",
                            new BsonDocument("wildcard", "*")
                        }
                        }
                    }
                }),
                new BsonDocument("$match", new BsonDocument{
                    { "user_id", _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) }
                })
            };

            var result = await _snippetCollection.Aggregate<Snippet>(pipeline).ToListAsync();
            return result;
        }
    }
}
