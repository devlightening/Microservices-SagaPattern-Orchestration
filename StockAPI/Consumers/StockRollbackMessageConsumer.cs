using MassTransit;
using MongoDB.Driver;
using Shared.Messages.RollbackMessage;
using StockAPI.Services;


namespace Stock.API.Consumers
{
    public class StockRollbackMessageConsumer(MongoDbService mongoDbService) : IConsumer<StockRollBackMessage>
    {
        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
        {
            var stockCollection = mongoDbService.GetCollection<StockAPI.Models.Entites.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await (await stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();

                stock.Count += orderItem.Count;
                await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
            }
        }
    }
}