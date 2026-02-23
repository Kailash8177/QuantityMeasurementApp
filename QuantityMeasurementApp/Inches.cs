using System;

namespace QuantityMeasurementApp
{
    public class Inches
    {
        private readonly double value;

        public Inches(double value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || this.GetType() != obj.GetType())
                return false;

            Inches other = (Inches)obj;

            return this.value.Equals(other.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}