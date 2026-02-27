using System;

namespace QuantityMeasurementApp
{
    /// <summary>
    /// Immutable value object representing a length measurement.
    /// Supports equality and explicit unit conversion.
    /// </summary>
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
        // UC5: Static Conversion API
        // ===============================
        public static double Convert(double value, LengthUnit source, LengthUnit target)
        {
            ValidateValue(value);
            ValidateUnit(source);
            ValidateUnit(target);

            // Normalize to base unit (Feet)
            double valueInFeet = value * source.ToFeetFactor();

            // Convert from base to target
            return valueInFeet / target.ToFeetFactor();
        }

        // ===============================
        // Instance Conversion
        // ===============================
        public QuantityLength ConvertTo(LengthUnit targetUnit)
        {
            double convertedValue = Convert(this.value, this.unit, targetUnit);
            return new QuantityLength(convertedValue, targetUnit);
        }

        // ===============================
        // Equality Logic (UC3/UC4)
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

        // ===============================
        // String Representation
        // ===============================
        public override string ToString()
        {
            return $"{value} {unit}";
        }

        // ===============================
        // Validation Methods
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