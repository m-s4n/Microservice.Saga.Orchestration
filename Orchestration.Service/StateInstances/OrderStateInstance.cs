using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchestration.Service.StateInstances
{
    // State Machine'ın yöneteceği veri yani sipariş verileri
    //iş gereksinimlerine göre gerekli veriler tutulur
    public class OrderStateInstance : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        // correlationId dışında hangi verileri tutabiliriz
        public string CurrentState { get; set; }
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }


    }
}
