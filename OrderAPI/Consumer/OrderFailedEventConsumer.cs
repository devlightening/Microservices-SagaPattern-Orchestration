using MassTransit;
using OrderAPI.Models.Context;
using OrderAPI.Models.Entites;
using Shared.Events.OrderEvents;

namespace OrderAPI.Consumer
{
    public class OrderFailedEventConsumer(OrderAPIDbContext orderAPIDbContext) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Order order = await orderAPIDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatu = Models.Enums.OrderStatusType.Failed;
                await orderAPIDbContext.SaveChangesAsync();


            }

        }
    }
}

