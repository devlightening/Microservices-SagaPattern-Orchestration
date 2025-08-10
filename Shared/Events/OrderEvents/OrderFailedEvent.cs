using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events.OrderEvents
{
    public class OrderFailedEvent
    {
        public int OrderId { get; set; }
        public string  Reason { get; set; }
    }
}
