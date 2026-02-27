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
        // PRIVATE BASE ADDITION HELPER
        // ===============================
        private static QuantityLength AddInternal(
            QuantityLength first,
            QuantityLength second,
            LengthUnit targetUnit)
        {
            if (first == null || second == null)
                throw new ArgumentException("Operands cannot be null");

            ValidateUnit(targetUnit);

            double firstInFeet = first.value * first.unit.ToFeetFactor();
            double secondInFeet = second.value * second.unit.ToFeetFactor();

            double sumInFeet = firstInFeet + secondInFeet;

            double resultValue = sumInFeet / targetUnit.ToFeetFactor();

            return new QuantityLength(resultValue, targetUnit);
        }

        // ===============================
        // UC6: Addition (Implicit Target = First Unit)
        // ===============================
        public QuantityLength Add(QuantityLength other)
        {
            return AddInternal(this, other, this.unit);
        }

        // ===============================
        // UC7: Addition with Explicit Target
        // ===============================
        public QuantityLength Add(QuantityLength other, LengthUnit targetUnit)
        {
            return AddInternal(this, other, targetUnit);
        }

        public static QuantityLength Add(
            QuantityLength first,
            QuantityLength second,
            LengthUnit targetUnit)
        {
            return AddInternal(first, second, targetUnit);
        }

        // ===============================
        // Equality
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