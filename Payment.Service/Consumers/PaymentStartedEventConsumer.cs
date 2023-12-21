using MassTransit;
using Shared.PaymentEvents;
using Shared.Settings;

namespace Payment.Service.Consumers
{
    public class PaymentStartedEventConsumer(ISendEndpointProvider _sendEndpointProvider) : IConsumer<PaymentStartedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId);

                await sendEndpoint.Send(paymentCompletedEvent);
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Bakiye yetersiz",
                    OrderItems = context.Message.OrderItems,
                };

                await sendEndpoint.Send(paymentFailedEvent);
            }
        }
    }
}
