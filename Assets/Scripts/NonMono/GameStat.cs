using System;

[Serializable]
public class GameStat {
    /// <summary>The internal name</summary>
    public readonly string name;
    /// <summary>Whether the stat has a persistent value across games</summary>
    public readonly bool persist;
    /// <summary>Whether the stat is allowed only to increase</summary>
    public readonly bool monotonic;

    private decimal value;
    private decimal persistantValue;
    [DontSaveField]
    [NonSerialized]
    private IStatPersistor persistor;

    public GameStat(string name, bool persist, bool monotonic) {
        this.name = name;
        this.persist = persist;
        this.monotonic = monotonic;
    }

    public void SetPersistor(IStatPersistor p) {
        persistor = p;
        if (persist) {
            persistantValue = p.GetValue(name);
        }
    }

    /// <summary>
    /// Adds the given value to the stat
    /// </summary>
    /// <param name="v"></param>
    public void Add(int v) {
        Add((decimal)v);
    }

    /// <summary>
    /// Adds the given value to the stat
    /// </summary>
    /// <param name="v"></param>
    public void Add(double v) {
        Add((decimal)v);
    }

    /// <summary>
    /// Adds the given value to the stat
    /// </summary>
    /// <param name="v"></param>
    public void Add(decimal v) {
        if (v < 0 && monotonic) {
            throw new ArgumentException("Monotonic stats cannot decrease");
        }
        value += v;
        if (persist) {
            persistantValue += v;
            persistor.SetValue(name, persistantValue);
        }   
    }

    /// <summary>
    /// The value as accumulated throughout this game
    /// </summary>
    public decimal Value {
        get {
            return value;
        }
    }

    /// <summary>
    /// The value as persisted across games
    /// </summary>
    public decimal PersistantValue {
        get {
            return persistantValue;
        }
    }
}
