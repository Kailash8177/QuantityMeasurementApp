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

        // Convert to another unit
        public QuantityLength ConvertTo(LengthUnit targetUnit)
        {
            double baseValue = unit.ConvertToBaseUnit(value);
            double converted = targetUnit.ConvertFromBaseUnit(baseValue);

            return new QuantityLength(converted, targetUnit);
        }

        // UC6 Addition (result in first operand unit)
        public QuantityLength Add(QuantityLength other)
        {
            return Add(other, this.unit);
        }

        // UC7 Addition with explicit target unit
        public QuantityLength Add(QuantityLength other, LengthUnit targetUnit)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            double base1 = unit.ConvertToBaseUnit(value);
            double base2 = other.unit.ConvertToBaseUnit(other.value);

            double sumBase = base1 + base2;

            double result = targetUnit.ConvertFromBaseUnit(sumBase);

            return new QuantityLength(result, targetUnit);
        }

        // Equality (UC1–UC4)
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