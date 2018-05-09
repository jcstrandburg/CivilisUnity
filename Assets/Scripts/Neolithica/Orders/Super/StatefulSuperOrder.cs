using System;
using System.Collections.Generic;
using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Base class for stateful orders with multiple steps
    /// </summary>
    [SerializableType]
    [SerializeDerivedType(10, typeof(ConstructOrder))]
    [SerializeDerivedType(11, typeof(FetchAvailableResourceOrder))]
    [SerializeDerivedType(12, typeof(FishOrder))]
    [SerializeDerivedType(13, typeof(HarvestFromReservoirOrder))]
    [SerializeDerivedType(14, typeof(HuntOrder))]
    [SerializeDerivedType(15, typeof(StoreCarriedResourceOrder))]
    [SerializeDerivedType(16, typeof(TransmuteOrder))]
    [SerializeDerivedType(17, typeof(UpgradeReservoirOrder))]
    public abstract class StatefulSuperOrder : BaseOrder {
        [SerializableMember(1)] public BaseOrder CurrentOrder;
        [SerializableMember(2)] public string CurrentState;

        protected abstract void CreateStates();

        public void CreateState(string stateName, Func<ActorController, BaseOrder> startState, Action<ActorController> completeState, Action<ActorController> failState) {
            var orderStateInfo = new OrderStateInfo(startState, completeState, failState);
            states.Add(stateName, orderStateInfo);
        }

        public override void DoStep(ActorController actor) {
            if (CurrentOrder == null) {
                Debug.Log($"{nameof(CurrentOrder)} is null");
                Failed = true;
                return;
            }

            CurrentOrder.DoStep(actor);

            if (!CurrentOrder.Done)
                return;

            OrderStateInfo info = States[CurrentState];
            if (CurrentOrder.Completed) {
                if (info.CompleteState != null) {
                    info.CompleteState(actor);
                }
                else {
                    Failed = true;
                    Debug.Log($"No complete transition available for state: {CurrentState}");
                }
            }
            else if (CurrentOrder.Failed) {
                if (info.FailState != null) {
                    info.FailState(actor);
                }
                else {
                    Failed = true;
                    Debug.Log($"No failure transition available for state: {CurrentState}");
                }
            }
            else if (CurrentOrder.Cancelled) {
                throw new InvalidOperationException("Order exectution cannot continue when sub order is cancelled!");
            }
        }

        /// <summary>Changes to the given state</summary>
        public void GoToState(string state, ActorController actor) {
            if (!States.ContainsKey(state))
                throw new ArgumentOutOfRangeException(nameof(state), $"Nonexistant order state: {state}");

            OrderStateInfo info = States[state];
            CurrentState = state;
            CurrentOrder = info.StartState(actor);
        }

        protected Dictionary<string, OrderStateInfo> States {
            get {
                if (states != null)
                    return states;

                states = new Dictionary<string, OrderStateInfo>();
                CreateStates();
                return states;
            }
        }

        private Dictionary<string, OrderStateInfo> states = null;
    }
}