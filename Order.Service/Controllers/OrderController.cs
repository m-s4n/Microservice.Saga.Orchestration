using Microsoft.AspNetCore.Mvc;
using Order.Service.DTOs;
using OrderEntity = Order.Service.DataAccess.Entities.Order;
using Order.Service.Enums;
using Order.Service.DataAccess.Entities;
using Order.Service.DataAccess.Contexts;
using MassTransit;
using Shared.OrderEvents;
using Shared.Messages;
using Shared.Settings;

namespace Order.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(
        AppDbContext _context, 
        ISendEndpointProvider _sendEndpointProvider
        ) : ControllerBase
    {
        [HttpPost("create-order")]
        public async Task CreateOrder(CreateOrderDto model)
        {
            OrderEntity order = new()
            {
                BuyerId = model.BuyerId,
                CreatedDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Suspend,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems.Select(oi => new OrderItem
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId,
                }).ToList(),
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // süreci başlatacak event oluşturulur
            OrderStartedEvent orderStartedEvent = new()
            {
                BuyerId = model.BuyerId,
                OrderId = order.Id,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                OrderItems = model.OrderItems.Select(oi => new OrderItemMessage
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId,
                }).ToList(),
            };

            // state machine'a event gönderilir.
            var sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new($"queue:{RabbitMQSettings.StateMachineQueue}"));
            await sendEndPoint.Send<OrderStartedEvent>(orderStartedEvent);
        }
    }
}
