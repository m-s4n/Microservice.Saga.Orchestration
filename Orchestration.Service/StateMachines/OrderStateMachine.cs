using MassTransit;
using Orchestration.Service.StateInstances;
using Shared.OrderEvents;
using Shared.StockEvents;
using Shared.PaymentEvents;
using Shared.Settings;
using Shared.Messages;

namespace Orchestration.Service.StateMachines
{
    public class OrderStateMachine:MassTransitStateMachine<OrderStateInstance>
    {
        // State instance'in hangi property'sinde durum ile ilgili bilgi tutulacak
        // State Machine'daki yapılacak çalışmadaki durum bilgilendirmesi OrderStateInstance içerisindeki
        // CurrentState isimli property'de tutulacak
        // Siparişe ait durum kontrolunu buradan yapacak
        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);
            // Gelen eventlere göre aksiyon alıyoruz
            // OrderStartedEvent gelirse yeni state instance oluşturulur
            // CorrelatedBy ile kıyas yapılır
            Event(() => OrderStartedEvent,
                orderStateInstance => orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid()));

            // StockReservedEvent gelirse
            // Buralarda hangi state'leri değiştirecek correlationId ile belirliyor
            Event(() => StockReservedEvent,
                orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            // StockNotReservedEvent gelirse
            Event(() => StockNotReservedEvent,
                orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            // PaymentCompletedEvent gelirse
            Event(() => PaymentCompletedEvent,
               orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            // PaymentFailedEvent gelirse
            Event(() => PaymentFailedEvent,
               orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            // Tetikleyici event gelirse
            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.UtcNow;
                })
                .TransitionTo(OrderCreated)
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems,
                }));

            // Diğer eventler için during kullanılır
            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
                context => new PaymentStartedEvent(context.Instance.CorrelationId)
                {
                    TotalPrice = context.Instance.TotalPrice,
                    OrderItems = context.Data.OrderItems
                })
                ,When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context => new OrderFailedEvent()
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                }));

            // ödeme başarısız olursa order'a  ve stock'a bilgi verilir.
            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                context => new OrderCompletedEvent()
                {
                    OrderId = context.Instance.OrderId
                })
                .Finalize(),

                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                context => new OrderFailedEvent
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                })
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}"),
                context => new StockRollbackMessage()
                {
                    OrderItems = context.Data.OrderItems
                }));

            // Başarılı olanları silmek istiyorsak
            SetCompletedWhenFinalized();
        }

        // State machine 'a gelebilecek event'leri tanımlıyoruz
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set;}
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        // State machine tarafından gönderilecekler eventler tanımlanmaz
        
        // Eventler geldiğinde state'ler değişecek bunun için state'ler tanımlanır
        // Her event'e karşılık bir state tanımlanır

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }

        // Tetikleyici event geldiğinde State Machine'da ilk karşılayıcı state tanımlanır
        // when fonksiyonu ile event kontrol edilir
        // then ile gerekli operasyon yapılır
        
        
    }
}
