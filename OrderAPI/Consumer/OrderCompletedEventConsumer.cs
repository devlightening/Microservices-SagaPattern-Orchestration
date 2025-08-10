using MassTransit;
using OrderAPI.Models.Context;
using OrderAPI.Models.Entites;
using Shared.Events.OrderEvents;

namespace OrderAPI.Consumer
{
    public class OrderCompletedEventConsumer(OrderAPIDbContext orderAPIDbContext) : IConsumer<OrderCompletedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            Order order = await orderAPIDbContext.Orders.FindAsync(context.Message.OrderId);
            if(order != null)
            {
                order.OrderStatu = Models.Enums.OrderStatusType.Completed;
                await orderAPIDbContext.SaveChangesAsync();


            }

        }
    }
}
