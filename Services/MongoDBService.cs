using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoExample.Models;

namespace MongoExample.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoCollection<Playlist> _playlistCollection;
    public MongoDbService(IOptions<MongoDbSettings> mongoDbSettings, IMongoClient client)
    {
        var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _playlistCollection = database.GetCollection<Playlist>(mongoDbSettings.Value.CollectionName);
    }

    public async Task<List<Playlist>> GetAsync()
    {
        return await _playlistCollection.Find(new BsonDocument()).ToListAsync().ConfigureAwait(false);
    }
    public async Task<Playlist> GetAsync(string id)
    {
        return await _playlistCollection.Find(Builders<Playlist>.Filter.Eq("Id", id)).FirstOrDefaultAsync().ConfigureAwait(false);
    }
    //await collection
    

    public async Task CreateAsync(Playlist playlist)
    {
        await _playlistCollection.InsertOneAsync(playlist).ConfigureAwait(false);
    }

    public async Task AddToPlaylistAsync(string id, string movieId)
    {
        var filter = Builders<Playlist>.Filter.Eq("Id", id);
        var update = Builders<Playlist>.Update.AddToSet("MovieIds", movieId);
        await _playlistCollection.UpdateOneAsync(filter, update).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<Playlist>.Filter.Eq("Id", id);
        await _playlistCollection.FindOneAndDeleteAsync(filter).ConfigureAwait(false);
    }
}