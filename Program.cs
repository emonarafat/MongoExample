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
