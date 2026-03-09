using System;

namespace QuantityMeasurementApp
{
    public class QuantityWeight : IEquatable<QuantityWeight>
    {
        private readonly double value;
        private readonly WeightUnit unit;

        private const double EPSILON = 1e-6;

        public QuantityWeight(double value, WeightUnit unit)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid value");

            this.value = value;
            this.unit = unit;
        }

        public double Value => value;
        public WeightUnit Unit => unit;

        public QuantityWeight ConvertTo(WeightUnit targetUnit)
        {
            double baseValue = unit.ConvertToBaseUnit(value);
            double converted = targetUnit.ConvertFromBaseUnit(baseValue);

            return new QuantityWeight(converted, targetUnit);
        }

        public QuantityWeight Add(QuantityWeight other)
        {
            return Add(other, this.unit);
        }

        public QuantityWeight Add(QuantityWeight other, WeightUnit targetUnit)
        {
            double base1 = unit.ConvertToBaseUnit(value);
            double base2 = other.unit.ConvertToBaseUnit(other.value);

            double sum = base1 + base2;

            double result = targetUnit.ConvertFromBaseUnit(sum);

            return new QuantityWeight(result, targetUnit);
        }

        public bool Equals(QuantityWeight other)
        {
            if (other == null)
                return false;

            double base1 = unit.ConvertToBaseUnit(value);
            double base2 = other.unit.ConvertToBaseUnit(other.value);

            return Math.Abs(base1 - base2) < EPSILON;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as QuantityWeight);
        }

        public override int GetHashCode()
        {
            return unit.ConvertToBaseUnit(value).GetHashCode();
        }

        public override string ToString()
        {
            return $"Quantity({value}, {unit})";
        }
    }
}