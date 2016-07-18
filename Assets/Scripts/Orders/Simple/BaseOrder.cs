using UnityEngine;
using System.Collections;

/// <summary>
/// The base for all actor orders
/// </summary>
[CustomSerialize]
public abstract class BaseOrder {
    public ActorController actor;
    public bool completed;
    public bool cancelled;
    public bool failed;
    public bool initialized;

    public bool Done {
        get {
            return completed || cancelled || failed;
        }
    }

    public BaseOrder(ActorController a) {
        actor = a;
        completed = cancelled = failed = false;
    }

    public void Update() {
        if (!initialized) {
            Initialize();
            initialized = true;
        }
        if (!Done) {
            DoStep();
        }
    }

    public virtual void Initialize() {
    }

    /// <summary>
    /// Does a single step for this order
    /// </summary>
    public abstract void DoStep();

    /// <summary>
    /// Cancels this order, freeing any resources as appropriate
    /// </summary>
    public virtual void Cancel() {
        cancelled = true;
    }

    public virtual void Pause() {
    }

    public virtual void Resume() {
    }
}
