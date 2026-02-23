using System;

namespace QuantityMeasurementApp
{
    public class QuantityLength : IEquatable<QuantityLength>
    {
        private readonly double value;
        private readonly LengthUnit unit;

        private const double INCH_TO_FEET = 1.0 / 12.0;
        private const double TOLERANCE = 0.0001;

        public QuantityLength(double value, LengthUnit unit)
        {
            if (!Enum.IsDefined(typeof(LengthUnit), unit))
                throw new ArgumentException("Invalid unit type");

            this.value = value;
            this.unit = unit;
        }

        private double ConvertToFeet()
        {
            return unit switch
            {
                LengthUnit.Feet => value,
                LengthUnit.Inch => value * INCH_TO_FEET,
                _ => throw new ArgumentException("Unsupported unit")
            };
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
    }
}