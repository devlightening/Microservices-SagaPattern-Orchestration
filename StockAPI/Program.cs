using MassTransit;
using MongoDB.Driver;
using StockAPI.Models.Entites;
using StockAPI.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {

        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddSingleton<MongoDbService>();


var app = builder.Build();


using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
if (!await (await mongoDbService.GetCollection<Stock>().FindAsync(x => true)).AnyAsync())
{
    mongoDbService.GetCollection<Stock>().InsertOne(new()
    {
        ProductId = 1,
        Count = 100
    });

    mongoDbService.GetCollection<Stock>().InsertOne(new()
    {
        ProductId = 2,
        Count = 100
    });
    mongoDbService.GetCollection<Stock>().InsertOne(new()
    {
        ProductId = 3,
        Count = 100
    });
    mongoDbService.GetCollection<Stock>().InsertOne(new()
    {
        ProductId = 4,
        Count = 100
    });
    mongoDbService.GetCollection<Stock>().InsertOne(new()
    {
        ProductId = 5,
        Count = 100
    });

}


app.Run();
