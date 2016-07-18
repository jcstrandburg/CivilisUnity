using System;

public abstract class DataBinding {
    public abstract void Update();
}

public class OneWayBinding<T> : DataBinding {
    Func<T> get;
    Action<T> set;
    T cachedValue;

    public OneWayBinding(Func<T> get, Action<T> set) {
        this.get = get;
        this.set = set;
        cachedValue = this.get();
        this.set(cachedValue);
    }

    public override void Update() {
        T val = get();
        if (!Equals(val, cachedValue)) {
            cachedValue = val;
            this.set(val);
        }
    }
}
