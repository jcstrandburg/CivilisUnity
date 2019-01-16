using System;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Utility struct for stateful super orders
    /// </summary>
    public struct OrderStateInfo {
        public Func<IOrderable, BaseOrder> StartState;
        public Action<IOrderable> CompleteState;
        public Action<IOrderable> FailState;

        public OrderStateInfo(Func<IOrderable, BaseOrder> start, Action<IOrderable> complete, Action<IOrderable> fail) {
            StartState = start;
            CompleteState = complete;
            FailState = fail;
        }
    }
}