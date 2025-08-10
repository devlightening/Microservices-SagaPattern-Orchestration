using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Settings
{
    public static class RabbitMQSettings
    {
        public const string StateMachineQueue = $"state_machine_queue";
        public const string Stock_OrderCreatedEventQueue = $"stokc_order_created_event_queue";
        public const string Stock_RollbackStockEventQueue = $"stock_rollback_stock_event_queue";
        public const string Order_OrderCompletedEventQueue = $"order_order_completed_event_queue";
        public const string Order_OrderFailedEventQueue = $"order_order_failed_event_queue";
        public const string Payment_StartedPaymentEventQueue = $"payment_started_payment_event_queue";




    }
}
