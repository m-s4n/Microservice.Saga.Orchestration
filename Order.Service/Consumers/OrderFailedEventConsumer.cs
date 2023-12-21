using MassTransit;
using Order.Service.DataAccess.Contexts;
using Order.Service.Enums;
using Shared.OrderEvents;
using OrderEntity = Order.Service.DataAccess.Entities.Order;

namespace Order.Service.Consumers
{
    public class OrderFailedEventConsumer(AppDbContext _context) : IConsumer<OrderFailedEvent>
    {
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {

            OrderEntity order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order is not null)
            {
                order.OrderStatus = OrderStatus.Fail;
                await _context.SaveChangesAsync();
            }
        }
    }
}
