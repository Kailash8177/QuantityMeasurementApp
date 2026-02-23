using System;

namespace QuantityMeasurementApp
{
    public class Feet
    {
        private readonly double value;

        public Feet(double value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            // Same reference
            if (this == obj)
                return true;

            // Null check
            if (obj == null)
                return false;

            // Type check
            if (this.GetType() != obj.GetType())
                return false;

            Feet other = (Feet)obj;

            // Floating point comparison
            return this.value.Equals(other.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}