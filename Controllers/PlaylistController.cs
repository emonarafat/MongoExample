using Microsoft.AspNetCore.Mvc;
using MongoExample.Models;
using MongoExample.Services;

namespace MongoExample.Controllers;

[Controller]
[Route("api/[controller]")]
public class PlaylistController : Controller
{
    private readonly IMongoDbService _mongoDbService;

    public PlaylistController(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpGet("{id}")]
    public async Task<Playlist> Get(string id)
    {
        return await _mongoDbService.GetAsync(id).ConfigureAwait(false);
    }

    [HttpGet]
    public async Task<List<Playlist>> Get()
    {
        return await _mongoDbService.GetAsync().ConfigureAwait(false);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Playlist playlist)
    {
        await _mongoDbService.CreateAsync(playlist).ConfigureAwait(false);
        return CreatedAtAction(nameof(Get), new { id = playlist.Id }, playlist);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AddToPlaylist(string id, [FromBody] string movieId)
    {
        await _mongoDbService.AddToPlaylistAsync(id, movieId).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mongoDbService.DeleteAsync(id).ConfigureAwait(false);
        return NoContent();
    }
}