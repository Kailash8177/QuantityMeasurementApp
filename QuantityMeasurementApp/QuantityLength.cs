using System;

namespace QuantityMeasurementApp
{
    public class QuantityLength : IEquatable<QuantityLength>
    {
        private readonly double value;
        private readonly LengthUnit unit;

        private const double TOLERANCE = 1e-6;

        public QuantityLength(double value, LengthUnit unit)
        {
            ValidateValue(value);
            ValidateUnit(unit);

            this.value = value;
            this.unit = unit;
        }

        public double Value => value;
        public LengthUnit Unit => unit;

        // ===============================
        // UC5: Static Conversion
        // ===============================
        public static double Convert(double value, LengthUnit source, LengthUnit target)
        {
            ValidateValue(value);
            ValidateUnit(source);
            ValidateUnit(target);

            double valueInFeet = value * source.ToFeetFactor();
            return valueInFeet / target.ToFeetFactor();
        }

        public QuantityLength ConvertTo(LengthUnit targetUnit)
        {
            double converted = Convert(this.value, this.unit, targetUnit);
            return new QuantityLength(converted, targetUnit);
        }

        // ===============================
        // UC6: Addition (Instance Method)
        // Result in unit of first operand
        // ===============================
        public QuantityLength Add(QuantityLength other)
        {
            if (other == null)
                throw new ArgumentException("Second operand cannot be null");

            // Convert both to base unit (Feet)
            double thisInFeet = this.value * this.unit.ToFeetFactor();
            double otherInFeet = other.value * other.unit.ToFeetFactor();

            double sumInFeet = thisInFeet + otherInFeet;

            // Convert back to unit of first operand
            double resultValue = sumInFeet / this.unit.ToFeetFactor();

            return new QuantityLength(resultValue, this.unit);
        }

        // ===============================
        // Optional Static Overload
        // ===============================
        public static QuantityLength Add(
            QuantityLength first,
            QuantityLength second)
        {
            if (first == null || second == null)
                throw new ArgumentException("Operands cannot be null");

            return first.Add(second);
        }

        // ===============================
        // Equality Logic (UC3+)
        // ===============================
        private double ConvertToFeet()
        {
            return value * unit.ToFeetFactor();
        }

        public bool Equals(QuantityLength other)
        {
            if (other == null)
                return false;

            return Math.Abs(this.ConvertToFeet() - other.ConvertToFeet()) < TOLERANCE;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            return Equals(obj as QuantityLength);
        }

        public override int GetHashCode()
        {
            return ConvertToFeet().GetHashCode();
        }

        public static bool operator ==(QuantityLength left, QuantityLength right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(QuantityLength left, QuantityLength right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{value} {unit}";
        }

        // ===============================
        // Validation
        // ===============================
        private static void ValidateValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid numeric value");
        }

        private static void ValidateUnit(LengthUnit unit)
        {
            if (!Enum.IsDefined(typeof(LengthUnit), unit))
                throw new ArgumentException("Invalid unit");
        }
    }
}