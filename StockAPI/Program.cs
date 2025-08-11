using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.API.Consumers;
using StockAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollbackMessageConsumer>();

    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);

        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));

        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackStockEventQueue, e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();

using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
if (!await (await mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>().FindAsync(x => true)).AnyAsync())
{
    mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>().InsertOne(new()
    {
        ProductId = 1,
        Count = 200
    });
    mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>().InsertOne(new()
    {
        ProductId = 2,
        Count = 300
    });
    mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>().InsertOne(new()
    {
        ProductId = 3,
        Count = 100
    });
    mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>().InsertOne(new()
    {
        ProductId = 4,
        Count = 100
    });
    mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>().InsertOne(new()
    {
        ProductId = 5,
        Count = 100
    });
}

app.Run();