using MongoExample.Models;

namespace MongoExample.Services;

public interface IMongoDbService
{
    Task<List<Playlist>> GetAsync();
    Task CreateAsync(Playlist playlist);
    Task AddToPlaylistAsync(string id, string movieId);
    Task DeleteAsync(string id);
    Task<Playlist> GetAsync(string id);
}