using System;

namespace QuantityMeasurementApp
{
    public class QuantityLength : IEquatable<QuantityLength>
    {
        private readonly double value;
        private readonly LengthUnit unit;

        private const double EPSILON = 1e-6;

        public QuantityLength(double value, LengthUnit unit)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid value");

            this.value = value;
            this.unit = unit;
        }

        public double Value => value;
        public LengthUnit Unit => unit;

        public QuantityLength ConvertTo(LengthUnit targetUnit)
        {
            double baseValue = unit.ConvertToBaseUnit(value);
            double converted = targetUnit.ConvertFromBaseUnit(baseValue);

            return new QuantityLength(converted, targetUnit);
        }

        public QuantityLength Add(QuantityLength other)
        {
            return Add(other, this.unit);
        }

        public QuantityLength Add(QuantityLength other, LengthUnit targetUnit)
        {
            double base1 = unit.ConvertToBaseUnit(value);
            double base2 = other.unit.ConvertToBaseUnit(other.value);

            double sum = base1 + base2;

            double result = targetUnit.ConvertFromBaseUnit(sum);

            return new QuantityLength(result, targetUnit);
        }

        public bool Equals(QuantityLength other)
        {
            if (other == null)
                return false;

            double base1 = unit.ConvertToBaseUnit(value);
            double base2 = other.unit.ConvertToBaseUnit(other.value);

            return Math.Abs(base1 - base2) < EPSILON;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as QuantityLength);
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