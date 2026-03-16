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

        // ==============================
        // Arithmetic Operation Enum
        // ==============================

        private enum ArithmeticOperation
        {
            ADD,
            SUBTRACT,
            DIVIDE
        }

        // ==============================
        // Validation
        // ==============================

        private void ValidateArithmeticOperands(Quantity<U> other, U targetUnit, bool targetRequired)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            if (Unit.GetType() != other.Unit.GetType())
                throw new ArgumentException("Measurement categories must match");

            if (targetRequired && targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");

            if (double.IsNaN(other.Value) || double.IsInfinity(other.Value))
                throw new ArgumentException("Invalid numeric value");
        }

        // ==============================
        // Conversion Helpers
        // ==============================

        private double ConvertToBaseUnit(double value, U unit)
        {
            if (unit is LengthUnit lu)
                return lu.ConvertToBaseUnit(value);

            if (unit is WeightUnit wu)
                return wu.ConvertToBaseUnit(value);

            if (unit is VolumeUnit vu)
                return vu.ConvertToBaseUnit(value);

            if (unit is TemperatureUnit tu)
                return tu.ConvertToBaseUnit(value);

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

            if (unit is TemperatureUnit tu)
                return tu.ConvertFromBaseUnit(baseValue);

            throw new ArgumentException("Unsupported unit type");
        }

        // ==============================
        // UC13 Central Arithmetic
        // ==============================

        private double PerformBaseArithmetic(Quantity<U> other, ArithmeticOperation operation)
        {
            // UC14 Temperature validation
            ((dynamic)Unit).ValidateOperationSupport(operation.ToString());

            double base1 = ConvertToBaseUnit(Value, Unit);
            double base2 = ConvertToBaseUnit(other.Value, other.Unit);

            switch (operation)
            {
                case ArithmeticOperation.ADD:
                    return base1 + base2;

                case ArithmeticOperation.SUBTRACT:
                    return base1 - base2;

                case ArithmeticOperation.DIVIDE:

                    if (base2 == 0)
                        throw new ArithmeticException("Division by zero");

                    return base1 / base2;

                default:
                    throw new ArgumentException("Unsupported operation");
            }
        }

        private double Round(double value)
        {
            return Math.Round(value, 2);
        }

        // ==============================
        // Conversion
        // ==============================

        public Quantity<U> ConvertTo(U targetUnit)
        {
            if (targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");

            double baseValue = ConvertToBaseUnit(Value, Unit);

            double result = ConvertFromBaseUnit(baseValue, targetUnit);

            return new Quantity<U>(Round(result), targetUnit);
        }

        // ==============================
        // ADD
        // ==============================

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

        // ==============================
        // SUBTRACT
        // ==============================

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

        // ==============================
        // DIVIDE
        // ==============================

        public double Divide(Quantity<U> other)
        {
            ValidateArithmeticOperands(other, default(U), false);

            return PerformBaseArithmetic(other, ArithmeticOperation.DIVIDE);
        }

        // ==============================
        // Equality
        // ==============================

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Quantity<U> other = (Quantity<U>)obj;

            if (Unit.GetType() != other.Unit.GetType())
                return false;

            double base1 = ConvertToBaseUnit(Value, Unit);
            double base2 = ConvertToBaseUnit(other.Value, other.Unit);

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