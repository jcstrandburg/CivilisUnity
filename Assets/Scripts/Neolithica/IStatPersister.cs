using System.Collections.Generic;

namespace Neolithica {
    public abstract class StatPersistor {
        protected Dictionary<string, decimal> values = new Dictionary<string, decimal>();

        public decimal GetValue(string name) {
            return values.ContainsKey(name) ? values[name] : 0m;
        }

        public void SetValue(string name, decimal value) {
            values[name] = value;
        }

        public abstract void ImportValues();
        public abstract void ExportValues();
        public abstract void Destroy();
    }
}