using MassTransit;
using SagaStateMatchineService.StateInstances;
using Shared.Events.OrderEvents;
using Shared.Events.PaymentEvents;
using Shared.Events.StockEvents;
using Shared.Messages.RollbackMessage;
using Shared.Settings;

namespace SagaStateMatchineService.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }


        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }

        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

            Event(() => OrderStartedEvent,
                orderStateInstance => orderStateInstance
                    .CorrelateById<int>(database => database.OrderId,
                        @event => @event.Message.OrderId)
                    .SelectId(e => Guid.NewGuid()));

            // Diğer Event tanımlamaları...

            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    // context.Data ve context.Instance'ın null olmadığına emin olmak için kontrol 
                    if (context.Data != null && context.Instance != null)
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.BuyerId = context.Data.BuyerId;
                        context.Instance.TotalPrice = context.Data.TotalPrice;
                        context.Instance.CreatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        // Eğer null gelirse ne olacağını belirlendi
                        // Örneğin, bir hata fırlatabilir veya loglayabilirsiniz.
                        throw new InvalidOperationException("OrderStartedEvent context data or instance is null.");
                    }
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                    context => new OrderCreatedEvent(context.Instance?.CorrelationId ?? Guid.NewGuid()) // Burada da null kontrolü 
                    {
                        OrderItems = context.Data?.OrderItems // OrderItems için de null kontrolü 

                    }));

            During(OrderCreated,
                When(StockReservedEvent)
                    .Then(context =>
                    {
                        // context.Data ve context.Instance'ın null olmadığına emin olmak için kontrol ekleyin
                        if (context.Data != null && context.Instance != null)
                        {
                            // İş mantığını burada uygulayacağız
                        }
                        else
                        {
                            throw new InvalidOperationException("StockReservedEvent context data or instance is null.");
                        }
                    })
                    .TransitionTo(StockReserved)
                    .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedPaymentEventQueue}"),
                        context => new PaymentStartedEvent(context.Instance?.CorrelationId ?? Guid.NewGuid())
                        {
                            OrderItems = context.Data?.OrderItems, // OrderItems için de null kontrolü 
                            TotalPrice = context.Instance?.TotalPrice ?? 0 // TotalPrice için de null kontrolü 

                        }),


                When(StockNotReservedEvent)
                    .Then(context =>
                    {
                        // İlgili loglama veya aksiyon
                        Console.WriteLine("Stock could not be reserved for order: " + context.Instance?.OrderId);
                    })
                    .TransitionTo(StockNotReserved)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                        context => new OrderFailedEvent
                        {
                            OrderId = context.Instance?.OrderId ?? 0,
                            Reason = context.Data?.Message ?? "Stock not reserved."
                        }));

            During(StockReserved,
                When(PaymentCompletedEvent)
                    .Then(context =>
                    {
                        // context.Data ve context.Instance'ın null olmadığına emin olmak için kontrol ekleyin
                        if (context.Data != null && context.Instance != null)
                        {
                            // İş mantığını burada uygulayacağız
                        }
                        else
                        {
                            throw new InvalidOperationException("PaymentCompletedEvent context data or instance is null.");
                        }
                    })
                    .TransitionTo(PaymentCompleted)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                        context => new OrderCompletedEvent
                        {
                            OrderId = context.Instance?.OrderId ?? 0,

                        }).Finalize(),
                When(PaymentFailedEvent)
                    .Then(context =>
                    {
                        // İlgili loglama veya aksiyon
                        Console.WriteLine("Payment failed for order: " + context.Instance?.OrderId);
                    })
                    .TransitionTo(PaymentFailed)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                        context => new OrderFailedEvent
                        {
                            OrderId = context.Instance?.OrderId ?? 0,
                            Reason = context.Data?.Message ?? "Payment failed."
                        })
                    .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackStockEventQueue}"),
                        context => new StockRollBackMessage
                        {
                            OrderItems = context.Data?.OrderItems // OrderItems için de null kontrolü 
                        }
                    ));
            SetCompletedWhenFinalized();

        }
    }
}
