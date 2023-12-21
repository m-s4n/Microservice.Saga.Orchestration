using MassTransit;
using Shared.OrderEvents;
using Stock.Service.Services;
using StockEntity = Stock.Service.DataAccess.Entities.Stock;
using MongoDB.Driver;
using Shared.Settings;
using Shared.StockEvents;

namespace Stock.Service.Consumers
{
    public class OrderCreatedEventConsumer(MongoDbService _mongoDbService, ISendEndpointProvider _sendEndpointProvider) : IConsumer<OrderCreatedEvent>
    {
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            var stockCollection = _mongoDbService.GetCollection<StockEntity>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                stockResult.Add(await (await stockCollection
                    .FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).AnyAsync());
            }

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));
            if(stockResult.TrueForAll(s => s.Equals(true)))
            {
                foreach(var orderItem in context.Message.OrderItems)
                {
                    var stock = await(await stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;

                    await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                }

                //// Stock başarılı
                //StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                //{
                //    OrderItems = context.Message.OrderItems,
                //};
                //await sendEndpoint.Send(stockReservedEvent);
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stock yetersiz"
                };
                await sendEndpoint.Send(stockNotReservedEvent);
            }
            else
            {
                // Stock başarısız
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stock yetersiz"
                };
                await sendEndpoint.Send(stockNotReservedEvent);
            }

        }
    }
}
