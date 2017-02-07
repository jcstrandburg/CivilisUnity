using System;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Utility struct for stateful super orders
    /// </summary>
    [Serializable]
    public struct OrderStateInfo {
        public Func<BaseOrder> startState;
        public Action completeState;
        public Action failState;

        public OrderStateInfo(Func<BaseOrder> start, Action complete, Action fail) {
            startState = start; completeState = complete; failState = fail;
        }
    }
}