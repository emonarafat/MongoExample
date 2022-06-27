Create a RESTful API with .NET Core and MongoDB
===============================================
[![.NET](https://github.com/emonarafat/MongoExample/actions/workflows/dotnet.yml/badge.svg)](https://github.com/emonarafat/MongoExample/actions/workflows/dotnet.yml)


In this tutorial, we're going to expand upon the previous and create a RESTful API with endpoints that perform basic create, read, update, and delete (CRUD) operations against MongoDB Atlas.

[](#the-requirements)

#### The Requirements

To be successful with this tutorial, you'll need to have a few things taken care of first:

* A deployed and configured MongoDB Atlas cluster, M0 or higher
    
* .NET Core 6+
    

We'll be using .NET Core 6.0 in this tutorial, but other versions may still work. Just take the version into consideration before continuing.

[](#create-a-web-api-project-with-the--net-core-cli)

#### Create a Web API Project with the .NET Core CLI

To kick things off, we're going to create a fresh .NET Core project using the web application template that Microsoft offers. To do this, execute the following commands from the CLI:
```console
dotnet new webapi -o MongoExample
cd MongoExample
dotnet add package MongoDB.Driver
```

The above commands will create a new web application project for .NET Core and install the latest MongoDB driver. We'll be left with some boilerplate files as part of the template, but we can remove them.

Inside the project, delete any file related to `WeatherForecast` and similar.

[](#designing-a-document-model-and-database-service-within--net-core)

#### Designing a Document Model and Database Service within .NET Core

Before we start designing each of the RESTful API endpoints with .NET Core, we need to create and configure our MongoDB service and define the data model for our API.

We'll start by working on our MongoDB service, which will be responsible for establishing our connection and directly working with documents within MongoDB. Within the project, create "Models/MongoDBSettings.cs" and add the following C# code:

```cs
namespace MongoExample.Models;

public class MongoDBSettings {

    public string ConnectionURI { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string CollectionName { get; set; } = null!;

}
```

The above `MongoDBSettings` class will hold information about our connection, the database name, and the collection name. The data we plan to store in these class fields will be found in the project's "appsettings.json" file. Open it and add the following:

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "MongoDB": {
        "ConnectionURI": "ATLAS_URI_HERE",
        "DatabaseName": "sample_mflix",
        "CollectionName": "playlist"
    }
}
```

Specifically take note of the `MongoDB` field. We'll be using the "sample_mflix" database and the "playlist" collection. You'll need to grab the `ConnectionURI` string from your MongoDB Atlas Dashboard.

![MongoDB Atlas Connection String](https://mongodb-devhub-cms.s3.us-west-1.amazonaws.com/mongodb_atlas_connection_string_801c3bdc01.jpg)

MongoDB Atlas Connection String

With the settings in place, we can move onto creating the service.

```cs
using MongoExample.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MongoExample.Services;

public class MongoDBService {

    private readonly IMongoCollection<Playlist> _playlistCollection;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings) {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _playlistCollection = database.GetCollection<Playlist>(mongoDBSettings.Value.CollectionName);
    }

    public async Task<List<Playlist>> GetAsync() { }
    public async Task CreateAsync(Playlist playlist) { }
    public async Task AddToPlaylistAsync(string id, string movieId) {}
    public async Task DeleteAsync(string id) { }

}
```

In the above code, each of the asynchronous functions were left blank on purpose. We'll be populating those functions as we create our endpoints. Instead, make note of the constructor method and how we're taking the passed settings that we saw in our "appsettings.json" file and setting them to variables. In the end, the only variable we'll ever interact with for this example is the `_playlistCollection` variable.

With the service available, we need to connect it to the application. Open the project's "Program.cs" file and add the following at the top:

```cs
using Microsoft.Extensions.Options;

using MongoDB.Driver;
using MongoExample.Models;
using MongoExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(s =>
{
   var mongoDbSettings= s.GetRequiredService<IOptions<MongoDbSettings>>();
   var client = new MongoClient(mongoDbSettings.Value.ConnectionUri);
   return client;
});
builder.Services.AddSingleton<IMongoDbService,MongoDbService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


```

You'll likely already have the `builder` variable in your code because it was part of the boilerplate project, so don't add it twice. What you'll need to add near the top is an import to your custom models and services as well as configuring the service.

Remember the `MongoDB` field in the "appsettings.json" file? That is the section that the `GetSection` function is pulling from. That information is passed into the singleton service that we created.

With the service created and working, with the exception of the incomplete asynchronous functions, we can focus on creating a data model for our collection.

Create "Models/Playlist.cs" and add the following C# code:
```cs
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoExample.Models;

public class Playlist {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Username { get; set; } = null!;

    [BsonElement("items")]
    [JsonPropertyName("items")]
    public List<string> MovieIds { get; set; } = null!;

}
```

There are a few things happening in the above class that take it from a standard C# class to something that can integrate seamlessly into a MongoDB document.

First, you might notice the following:

```cs
[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string? Id { get; set; }
```

We're saying that the `Id` field is to be represented as an ObjectId in BSON and the `_id` field within MongoDB. However, when we work with it locally in our application, it will be a string.

The next thing you'll notice is the following:

```cs
[BsonElement("items")]
[JsonPropertyName("items")]
public List<string> movieIds { get; set; } = null!;
```

Even though we plan to work with `movieIds` within our C# application, in MongoDB, the field will be known as `items` and when sending or receiving JSON, the field will also be known as `items` instead of `movieIds`.

You don't need to define custom mappings if you plan to have your local class field match the document field directly. Take the `username` field in our example. It has no custom mappings, so it will be `username` in C#, `username` in JSON, and `username` in MongoDB.

Just like that, we have a MongoDB service and document model for our collection to work with for .NET Core.

[](#building-crud-endpoints-that-interact-with-mongodb-using--net-core)

#### Building CRUD Endpoints that Interact with MongoDB Using .NET Core

When building CRUD endpoints for this project, we'll need to bounce between two different locations within our project. We'll need to define the endpoint within a controller and do the work within our service.

Create "Controllers/PlaylistController.cs" and add the following code:

```cs
using System;
using Microsoft.AspNetCore.Mvc;
using MongoExample.Services;
using MongoExample.Models;

namespace MongoExample.Controllers; 

[Controller]
[Route("api/[controller]")]
public class PlaylistController: Controller {
    
    private readonly MongoDBService _mongoDBService;

    public PlaylistController(MongoDBService mongoDBService) {
        _mongoDBService = mongoDBService;
    }

    [HttpGet]
    public async Task<List<Playlist>> Get() {}

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Playlist playlist) {}

    [HttpPut("{id}")]
    public async Task<IActionResult> AddToPlaylist(string id, [FromBody] string movieId) {}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id) {}

}
```

In the above `PlaylistController` class, we have a constructor method that gains access to our singleton service class. Then we have a series of endpoints for this particular controller. We could add far more endpoints than this to our controller, but it's not necessary for this example.

Let's start with creating data through the POST endpoint. To do this, it's best to start in the "Services/MongoDBService.cs" file:
```cs
public async Task CreateAsync(Playlist playlist) {
    await _playlistCollection.InsertOneAsync(playlist);
    return;
}
```

We had set the `_playlistCollection` in the constructor method of the service, so we can now use the `InsertOneAsync` method, taking a passed `Playlist` variable and inserting it. Jumping back into the "Controllers/PlaylistController.cs," we can add the following:

```cs
[HttpPost]
public async Task<IActionResult> Post([FromBody] Playlist playlist) {
    await _mongoDBService.CreateAsync(playlist);
    return CreatedAtAction(nameof(Get), new { id = playlist.Id }, playlist);
}
```

What we're saying is that when the endpoint is executed, we take the `Playlist` object from the request, something that .NET Core parses for us, and pass it to the `CreateAsync` function that we saw in the service. After the insert, we return some information about the interaction.

It's important to note that in this example project, we won't be validating any data flowing from HTTP requests.

Let's jump to the read operations.

Head back into the "Services/MongoDBService.cs" file and add the following function:

```cs
public async Task<List<Playlist>> GetAsync() {
    return await _playlistCollection.Find(new BsonDocument()).ToListAsync();
}
```

The above `Find` operation will return all documents that exist in the collection. If you wanted to, you could make use of the `FindOne` or provide filter criteria to return only the data that you want. We'll explore filters shortly.

With the service function ready, add the following endpoint to the "Controllers/PlaylistController.cs" file:

```cs
[HttpGet]
public async Task<List<Playlist>> Get() {
    return await _mongoDBService.GetAsync();
}
```

Not so bad, right? We'll be doing the same thing for the other endpoints, more or less.

The next CRUD stage to take care of is the updating of data. Within the "Services/MongoDBService.cs" file, add the following function:
```cs
public async Task AddToPlaylistAsync(string id, string movieId) {
    FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
    UpdateDefinition<Playlist> update = Builders<Playlist>.Update.AddToSet<string>("movieIds", movieId);
    await _playlistCollection.UpdateOneAsync(filter, update);
    return;
}
```

Rather than making changes to the entire document, we're planning on adding an item to our playlist and nothing more. To do this, we set up a match filter to determine which document or documents should receive the update. In this case, we're matching on the id which is going to be unique. Next, we're defining the update criteria, which is an `AddToSet` operation that will only add an item to the array if it doesn't already exist in the array.

The `UpdateOneAsync` method will only update one document even if the match filter returned more than one match.

In the "Controllers/PlaylistController.cs" file, add the following endpoint to pair with the `AddToPlayListAsync` function:

```cs
[HttpPut("{id}")]
public async Task<IActionResult> AddToPlaylist(string id, [FromBody] string movieId) {
    await _mongoDBService.AddToPlaylistAsync(id, movieId);
    return NoContent();
}
```

In the above PUT endpoint, we are taking the `id` from the route parameters and the `movieId` from the request body and using them with the `AddToPlaylistAsync` function.

This brings us to our final part of the CRUD spectrum. We're going to handle deleting of data.

In the "Services/MongoDBService.cs" file, add the following function:

```cs
public async Task DeleteAsync(string id) {
    FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
    await _playlistCollection.DeleteOneAsync(filter);
    return;
}
```

The above function will delete a single document based on the filter criteria. The filter criteria, in this circumstance, is a match on the id which is always going to be unique. Your filters could be more extravagant if you wanted.

To bring it to an end, the endpoint for this function would look like the following in the "Controllers/PlaylistController.cs" file:
```cs
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id) {
    await _mongoDBService.DeleteAsync(id);
    return NoContent();
}
```

We only created four endpoints, but you could take everything we did and create 100 more if you wanted to. They would all use a similar strategy and can leverage everything that MongoDB has to offer.

[](#conclusion)

#### Conclusion

You just saw how to create a simple four endpoint RESTful API using .NET Core and MongoDB. 

Like I mentioned, you can take the same strategy used here and apply it towards more endpoints, each doing something critical for your web application.

Got a question about the driver for .NET? Swing by the [MongoDB Community Forums](https://community.mongodb.com)!
