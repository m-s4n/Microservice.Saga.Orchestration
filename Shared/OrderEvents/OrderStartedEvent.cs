using Shared.Messages;


namespace Shared.OrderEvents
{
    // süreci başlatacak state oluşturacak event
    // state instance'in istediği veriler olacak
    public class OrderStartedEvent
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        // Hangi ürünlerin sipariş edildiği ihtiyaç olabilir
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
