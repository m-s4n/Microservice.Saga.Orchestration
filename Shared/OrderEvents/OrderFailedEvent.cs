using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.OrderEvents
{
    public class OrderFailedEvent
    {
        public int OrderId { get; set; }
        // Neden başarısız oldu
        public string Message { get; set; }
    }
}
