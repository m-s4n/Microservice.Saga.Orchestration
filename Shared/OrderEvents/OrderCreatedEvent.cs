using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.OrderEvents
{
    // Oluşturulan siparişe dair bilgileri ilgili servislere göndereceğiz
    // CorrelationId yi kullanmak için masstrasit yüklenir
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get;}
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
