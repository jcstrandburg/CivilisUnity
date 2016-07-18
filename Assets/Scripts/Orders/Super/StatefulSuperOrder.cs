using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for stateful orders with multiple steps
/// </summary>
public abstract class StatefulSuperOrder : BaseOrder {
    public BaseOrder currentOrder = null;
    public string currentState;
    [DontSaveField]
    private IDictionary<string, OrderStateInfo> states = null;

    protected IDictionary<string, OrderStateInfo> States {
        get {
            if (states == null) {
                states = new Dictionary<string, OrderStateInfo>();
                CreateStates();
            }
            return states;
        }
    }

    public StatefulSuperOrder(ActorController a) : base(a) {
    }

    protected abstract void CreateStates();

    public void CreateState(string stateName, Func<BaseOrder> startState, Action completeState, Action failState) {
        OrderStateInfo info = new OrderStateInfo(startState, completeState, failState);
        states.Add(stateName, info);
    }

    public override void DoStep() {
        if (currentOrder != null) {
            currentOrder.DoStep();

            //check to see if order is done somehow
            if (currentOrder.Done) {
                OrderStateInfo info = States[currentState];
                if (currentOrder.completed) {
                    if (info.completeState != null) {
                        info.completeState();
                    }
                    else {
                        this.failed = true;
                        Debug.Log("No complete transition available for state: " + currentState);
                    }
                }
                else if (currentOrder.failed) {
                    if (info.failState != null) {
                        info.failState();
                    }
                    else {
                        this.failed = true;
                        Debug.Log("No failure transition available for state: " + currentState);
                    }
                }
                else if (currentOrder.cancelled) {
                    throw new Exception("Order exectution cannot continue when sub order is cancelled!");
                }
            }
        }
        else {
            Debug.Log("currentOrder is null!");
            this.failed = true;
        }
    }

    /// <summary>Changes to the given state</summary>
    public void GoToState(string state) {
        if (States.ContainsKey(state)) {
            OrderStateInfo info = States[state];
            currentState = state;
            currentOrder = info.startState();
        }
        else {
            throw new ArgumentOutOfRangeException("Nonexistant order state: " + state);
        }
    }
}