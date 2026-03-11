using System;
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

        private double ConvertToBase()
        {
            if (Unit is LengthUnit lu)
                return lu.ConvertToBaseUnit(Value);

            if (Unit is WeightUnit wu)
                return wu.ConvertToBaseUnit(Value);

            if (Unit is VolumeUnit vu)
                return vu.ConvertToBaseUnit(Value);

            throw new ArgumentException("Unsupported unit type");
        }

        private double ConvertFromBase(double baseValue, U targetUnit)
        {
            if (targetUnit is LengthUnit lu)
                return lu.ConvertFromBaseUnit(baseValue);

            if (targetUnit is WeightUnit wu)
                return wu.ConvertFromBaseUnit(baseValue);

            if (targetUnit is VolumeUnit vu)
                return vu.ConvertFromBaseUnit(baseValue);

            throw new ArgumentException("Unsupported unit type");
        }

        public Quantity<U> ConvertTo(U targetUnit)
        {
            double baseValue = ConvertToBase();
            double result = ConvertFromBase(baseValue, targetUnit);

            return new Quantity<U>(Math.Round(result, 2), targetUnit);
        }

        public Quantity<U> Add(Quantity<U> other, U targetUnit)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();

            double sum = base1 + base2;

            double result = ConvertFromBase(sum, targetUnit);

            return new Quantity<U>(Math.Round(result, 2), targetUnit);
        }

        // ---------------------------
        // UC12: SUBTRACTION
        // ---------------------------

        public Quantity<U> Subtract(Quantity<U> other)
        {
            return Subtract(other, Unit);
        }

        public Quantity<U> Subtract(Quantity<U> other, U targetUnit)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            if (targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");

            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();

            double difference = base1 - base2;

            double result = ConvertFromBase(difference, targetUnit);

            return new Quantity<U>(Math.Round(result, 2), targetUnit);
        }

        // ---------------------------
        // UC12: DIVISION
        // ---------------------------

        public double Divide(Quantity<U> other)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();

            if (base2 == 0)
                throw new ArithmeticException("Division by zero");

            return base1 / base2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Quantity<U>)obj;

            double base1 = ConvertToBase();
            double base2 = other.ConvertToBase();

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