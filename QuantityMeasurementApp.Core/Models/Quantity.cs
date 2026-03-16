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
                throw new ArgumentException("Invalid numeric value");

            Value = value;
            Unit = unit;
        }

        // ============================
        // Arithmetic Operation Enum
        // ============================

        private enum ArithmeticOperation
        {
            ADD,
            SUBTRACT,
            DIVIDE
        }

        // ============================
        // Validation Helper (DRY)
        // ============================

        private void ValidateArithmeticOperands(Quantity<U> other, U targetUnit, bool targetUnitRequired)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            if (double.IsNaN(other.Value) || double.IsInfinity(other.Value))
                throw new ArgumentException("Invalid numeric value");

            if (Unit.GetType() != other.Unit.GetType())
                throw new ArgumentException("Measurement categories must match");

            if (targetUnitRequired && targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");
        }

        // ============================
        // Base Conversion
        // ============================

        private double ConvertToBaseUnit(double value, U unit)
        {
            if (unit is LengthUnit lu)
                return lu.ConvertToBaseUnit(value);

            if (unit is WeightUnit wu)
                return wu.ConvertToBaseUnit(value);

            if (unit is VolumeUnit vu)
                return vu.ConvertToBaseUnit(value);

            throw new ArgumentException("Unsupported unit type");
        }

        private double ConvertFromBaseUnit(double baseValue, U unit)
        {
            if (unit is LengthUnit lu)
                return lu.ConvertFromBaseUnit(baseValue);

            if (unit is WeightUnit wu)
                return wu.ConvertFromBaseUnit(baseValue);

            if (unit is VolumeUnit vu)
                return vu.ConvertFromBaseUnit(baseValue);

            throw new ArgumentException("Unsupported unit type");
        }

        // ============================
        // Central Arithmetic Logic
        // ============================

        private double PerformBaseArithmetic(Quantity<U> other, ArithmeticOperation operation)
        {
            double baseValue1 = ConvertToBaseUnit(Value, Unit);
            double baseValue2 = ConvertToBaseUnit(other.Value, other.Unit);

            switch (operation)
            {
                case ArithmeticOperation.ADD:
                    return baseValue1 + baseValue2;

                case ArithmeticOperation.SUBTRACT:
                    return baseValue1 - baseValue2;

                case ArithmeticOperation.DIVIDE:

                    if (baseValue2 == 0)
                        throw new ArithmeticException("Division by zero");

                    return baseValue1 / baseValue2;

                default:
                    throw new ArgumentException("Unsupported operation");
            }
        }

        private double Round(double value)
        {
            return Math.Round(value, 2);
        }

        // ============================
        // ADD
        // ============================

        public Quantity<U> Add(Quantity<U> other)
        {
            return Add(other, Unit);
        }

        public Quantity<U> Add(Quantity<U> other, U targetUnit)
        {
            ValidateArithmeticOperands(other, targetUnit, true);

            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.ADD);

            double result = ConvertFromBaseUnit(baseResult, targetUnit);

            return new Quantity<U>(Round(result), targetUnit);
        }

        // ============================
        // SUBTRACT
        // ============================

        public Quantity<U> Subtract(Quantity<U> other)
        {
            return Subtract(other, Unit);
        }

        public Quantity<U> Subtract(Quantity<U> other, U targetUnit)
        {
            ValidateArithmeticOperands(other, targetUnit, true);

            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.SUBTRACT);

            double result = ConvertFromBaseUnit(baseResult, targetUnit);

            return new Quantity<U>(Round(result), targetUnit);
        }

        // ============================
        // DIVIDE
        // ============================

        public double Divide(Quantity<U> other)
        {
            ValidateArithmeticOperands(other, default(U), false);

            return PerformBaseArithmetic(other, ArithmeticOperation.DIVIDE);
        }

        // ============================
        // CONVERSION
        // ============================

        public Quantity<U> ConvertTo(U targetUnit)
        {
            if (targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");

            double baseValue = ConvertToBaseUnit(Value, Unit);

            double result = ConvertFromBaseUnit(baseValue, targetUnit);

            return new Quantity<U>(Round(result), targetUnit);
        }

        // ============================
        // EQUALITY
        // ============================

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Quantity<U> other = (Quantity<U>)obj;

            double baseValue1 = ConvertToBaseUnit(Value, Unit);
            double baseValue2 = ConvertToBaseUnit(other.Value, other.Unit);

            return Math.Abs(baseValue1 - baseValue2) < 0.0001;
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