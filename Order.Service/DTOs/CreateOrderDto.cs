namespace Order.Service.DTOs
{
    public record CreateOrderDto(int BuyerId, ICollection<OrderItemDto> OrderItems);
    
}
