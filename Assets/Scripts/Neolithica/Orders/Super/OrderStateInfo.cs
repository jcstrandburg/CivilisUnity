using System;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Utility struct for stateful super orders
    /// </summary>
    public struct OrderStateInfo {
        public Func<ActorController, BaseOrder> StartState;
        public Action<ActorController> CompleteState;
        public Action<ActorController> FailState;

        public OrderStateInfo(Func<ActorController, BaseOrder> start, Action<ActorController> complete, Action<ActorController> fail) {
            StartState = start;
            CompleteState = complete;
            FailState = fail;
        }
    }
}