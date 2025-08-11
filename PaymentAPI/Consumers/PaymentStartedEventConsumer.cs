using MassTransit;
using Shared.Events.PaymentEvents;
using Shared.Settings;

namespace PaymentAPI.Consumers
{
    public class PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider) : IConsumer<PaymentStartedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var sendEndPoint = sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));
            if (false)
            {
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId)
                {

                };

                await sendEndpointProvider.Send(paymentCompletedEvent);

            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Yetersiz Bakiye!!",
                    OrderItems = context.Message.OrderItems

                };
                await sendEndpointProvider.Send(paymentFailedEvent);
            }

        }
    }
}
