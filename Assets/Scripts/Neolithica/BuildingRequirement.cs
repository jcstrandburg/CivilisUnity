using System;

namespace Neolithica {
    [Serializable]
    public class BuildingRequirement : ICloneable {
        public string name;
        public double amount;
        public double epsilon=0.01;
        public Comparison comparison = Comparison.GreaterOrEqual;

        public object Clone() {
            var br = new BuildingRequirement();
            br.name = this.name;
            br.amount = this.amount;
            return br;
        }

        public bool IsSatisfied(float compareAmount) {
            switch (comparison) {
                case Comparison.LessThan:
                    return compareAmount < amount;
                case Comparison.LesserOrEqual:
                    return compareAmount <= amount;
                case Comparison.Equal:
                    return compareAmount >= (amount-epsilon) && compareAmount <= (amount + epsilon);
                case Comparison.GreaterOrEqual:
                    return compareAmount >= amount;
                case Comparison.GreaterThan:
                    return compareAmount > amount;
                default:
                    throw new InvalidOperationException("Unhandled comparison value" + comparison);
            }
        }
    }
}