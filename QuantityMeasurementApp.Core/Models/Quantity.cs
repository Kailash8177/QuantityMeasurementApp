using QuantityMeasurementApp.Core.Units;

namespace QuantityMeasurementApp.Core.Models
{
    public class Quantity<U>
    {
        public double Value { get; }
        public U Unit { get; }

        public Quantity(double value, U unit)
        {
            if (unit == null)
                throw new ArgumentException("Unit cannot be null");

            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid value");

            Value = value;
            Unit = unit;
        }

        public Quantity<U> ConvertTo(U targetUnit)
        {
            double baseValue;

            if (Unit is LengthUnit lu)
                baseValue = lu.ConvertToBaseUnit(Value);
            else if (Unit is WeightUnit wu)
                baseValue = wu.ConvertToBaseUnit(Value);
            else
                throw new Exception("Unsupported unit");

            double converted;

            if (targetUnit is LengthUnit tlu)
                converted = tlu.ConvertFromBaseUnit(baseValue);
            else if (targetUnit is WeightUnit twu)
                converted = twu.ConvertFromBaseUnit(baseValue);
            else
                throw new Exception("Unsupported unit");

            return new Quantity<U>(Math.Round(converted, 2), targetUnit);
        }

        public Quantity<U> Add(Quantity<U> other, U targetUnit)
        {
            double base1;
            double base2;

            if (Unit is LengthUnit lu)
                base1 = lu.ConvertToBaseUnit(Value);
            else if (Unit is WeightUnit wu)
                base1 = wu.ConvertToBaseUnit(Value);
            else
                throw new Exception("Unsupported unit");

            if (other.Unit is LengthUnit olu)
                base2 = olu.ConvertToBaseUnit(other.Value);
            else if (other.Unit is WeightUnit owu)
                base2 = owu.ConvertToBaseUnit(other.Value);
            else
                throw new Exception("Unsupported unit");

            double sumBase = base1 + base2;

            double result;

            if (targetUnit is LengthUnit tlu)
                result = tlu.ConvertFromBaseUnit(sumBase);
            else if (targetUnit is WeightUnit twu)
                result = twu.ConvertFromBaseUnit(sumBase);
            else
                throw new Exception("Unsupported unit");

            return new Quantity<U>(Math.Round(result, 2), targetUnit);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Quantity<U>)obj;

            double base1;
            double base2;

            if (Unit is LengthUnit lu)
                base1 = lu.ConvertToBaseUnit(Value);
            else if (Unit is WeightUnit wu)
                base1 = wu.ConvertToBaseUnit(Value);
            else
                throw new Exception("Unsupported unit");

            if (other.Unit is LengthUnit olu)
                base2 = olu.ConvertToBaseUnit(other.Value);
            else if (other.Unit is WeightUnit owu)
                base2 = owu.ConvertToBaseUnit(other.Value);
            else
                throw new Exception("Unsupported unit");

            return Math.Abs(base1 - base2) < 0.0001;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Quantity({Value}, {Unit})";
        }
    }
}