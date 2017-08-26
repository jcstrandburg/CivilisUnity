using System;
using AqlaSerializer;

namespace Neolithica {
    public delegate void IGameStatChangeListener(IGameStat s);

    public interface IGameStat {
        event IGameStatChangeListener OnChange;

        /// <summary>
        /// The value as accumulated throughout this game
        /// </summary>
        decimal Value { get; }

        /// <summary>
        /// The value as persisted across games
        /// </summary>
        decimal PersistantValue { get; }

        /// <summary>
        /// Adds the given value to the stat
        /// </summary>
        /// <param name="v"></param>
        void Add(int v);

        /// <summary>
        /// Adds the given value to the stat
        /// </summary>
        /// <param name="v"></param>
        void Add(double v);

        /// <summary>
        /// Adds the given value to the stat
        /// </summary>
        /// <param name="v"></param>
        void Add(decimal v);
    }

    [SerializableType]
    public class GameStat : IGameStat {

        public event IGameStatChangeListener OnChange;

        /// <summary>The internal name</summary>
        [SerializableMember(1)] public readonly string Name;
        /// <summary>Whether the stat has a persistent value across games</summary>
        [SerializableMember(2)] public readonly bool Persist;
        /// <summary>Whether the stat is allowed only to increase</summary>
        [SerializableMember(3)] public readonly bool Monotonic;

        /// <summary>
        /// The value as accumulated throughout this game
        /// </summary>
        public decimal Value => value;

        /// <summary>
        /// The value as persisted across games
        /// </summary>
        public decimal PersistantValue => persistantValue;

        public GameStat(string name, bool persist, bool monotonic) {
            Name = name;
            Persist = persist;
            Monotonic = monotonic;
        }

        public void SetPersistor(StatPersistor p) {
            persistor = p;

            if (Persist) {
                persistantValue = p.GetValue(Name);
            }
        }

        public void Add(int v) {
            Add((decimal)v);
        }

        public void Add(double v) {
            Add((decimal)v);
        }

        public void Add(decimal v) {
            if (v < 0 && Monotonic)
                throw new ArgumentException("Monotonic stats cannot decrease", nameof(v));

            if (v != 0) {
                value += v;

                if (Persist) {
                    persistantValue += v;
                    persistor.SetValue(Name, persistantValue);
                }

                if (OnChange != null) {
                    OnChange(this);
                }
            }
        }

        /// <summary>
        /// Method for overriding value rather than incrementing it. To be used internally by <c>StatManager</c> (not exposed via <c>IGameStat</c>).
        /// Does not trigger event listiners.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(decimal value) {
            this.value = value;
        }

        private decimal value;
        private decimal persistantValue;
        private StatPersistor persistor;
    }
}
