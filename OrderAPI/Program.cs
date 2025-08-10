using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Models.Context;
using OrderAPI.Models.Entites;
using OrderAPI.Models.Enums;
using OrderAPI.ViewModels;
using Shared.Events.OrderEvents;
using Shared.Messages;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

// Swagger ve OpenAPI desteðini ekler
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMassTransit(configurator =>
{
    //configurator.AddConsumer<PaymentCompletedEventConsumer>();
    //configurator.AddConsumer<PaymentFailedEventConsumer>();
    //configurator.AddConsumer<StockNotReservedEventConsume>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        //_configure.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
        //_configure.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue, e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
        //_configure.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue, e => e.ConfigureConsumer<StockNotReservedEventConsume>(context));
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddDbContext<OrderAPIDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

var app = builder.Build();

// Geliþtirme ortamýnda Swagger'ý etkinleþtirir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ----------------------------------------------------

app.MapPost("/create-order", async (CreateOrderViewModel model, OrderAPIDbContext context, ISendEndpointProvider sendEndpointProvider) =>
{
    Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatu = OrderStatusType.Suspend,
        TotalPrice = model.OrderItems.Sum(item => item.Price * item.Count),
        OrderItems = model.OrderItems.Select(item => new OrderItem
        {
            ProductId = item.ProductId,
            Count = item.Count,
            Price = item.Price
        }).ToList(),
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderStartedEvent orderStartedEvent = new()
    {
        OrderId = order.OrderId,
        BuyerId = order.BuyerId,
        TotalPrice = model.OrderItems.Sum(item => item.Price * item.Count),
        OrderItems = order.OrderItems.Select(item => new OrderItemMessage
        {
            ProductId = item.ProductId,
            Count = item.Count,
            Price = item.Price
        }).ToList(),
    };
    var sendEndPoint = await sendEndpointProvider.GetSendEndpoint
                    (new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

    await sendEndPoint.Send<OrderStartedEvent>(orderStartedEvent);

});


app.Run();