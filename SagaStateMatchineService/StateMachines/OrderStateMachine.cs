using MassTransit;
using SagaStateMatchineService.StateInstances;

namespace SagaStateMatchineService.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

        }
    }
}
