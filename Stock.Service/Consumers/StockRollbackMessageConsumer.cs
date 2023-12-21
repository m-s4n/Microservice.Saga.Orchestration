using MassTransit;
using Shared.Messages;
using Stock.Service.Services;
using StockEntity = Stock.Service.DataAccess.Entities.Stock;
using MongoDB.Driver;

namespace Stock.Service.Consumers
{
    public class StockRollbackMessageConsumer(MongoDbService _mongoDbService) : IConsumer<StockRollbackMessage>
    {
        public async Task Consume(ConsumeContext<StockRollbackMessage> context)
        {
            var stockCollection = _mongoDbService.GetCollection<StockEntity>();
            foreach(var orderItem in context.Message.OrderItems)
            {
                var stock = await (await stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                stock.Count += orderItem.Count;
                await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
            }
        }
    }
}
