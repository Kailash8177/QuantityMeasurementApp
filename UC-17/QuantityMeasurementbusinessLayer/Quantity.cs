using System;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer
{
    /// <summary>
    /// Generic measurement class. U must implement IMeasurable.
    /// Delegates all conversion math to the IMeasurable implementation —
    /// the core arithmetic pattern from UC15 / UC16.
    /// </summary>
    public class Quantity<U> where U : IMeasurable
    {
        public double Value { get; }
        public U      Unit  { get; }

        public Quantity(double value, U unit)
        {
            if (unit == null)
                throw new ArgumentException("Unit cannot be null");
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid numeric value");

            Value = value;
            Unit  = unit;
        }

        // ── Validation ────────────────────────────────────────────────

        private void ValidateArithmeticOperands(Quantity<U> other, bool targetRequired, U targetUnit)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");
            if (double.IsNaN(other.Value) || double.IsInfinity(other.Value))
                throw new ArgumentException("Invalid numeric value");
            if (Unit.GetCategory() != other.Unit.GetCategory())
                throw new ArgumentException("Measurement categories must match");
            if (targetRequired && targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");
        }

        // ── Core arithmetic (all in base units) ───────────────────────

        private double PerformBaseArithmetic(Quantity<U> other, ArithmeticOperation operation)
        {
            Unit.ValidateOperationSupport(operation.ToString());

            double base1 = Unit.ConvertToBaseUnit(Value);
            double base2 = other.Unit.ConvertToBaseUnit(other.Value);

            return operation switch
            {
                ArithmeticOperation.ADD      => base1 + base2,
                ArithmeticOperation.SUBTRACT => base1 - base2,
                ArithmeticOperation.DIVIDE   => base2 == 0
                    ? throw new ArithmeticException("Division by zero")
                    : base1 / base2,
                _ => throw new ArgumentException("Unsupported operation")
            };
        }

        private static double Round(double v) => Math.Round(v, 2);

        // ── ConvertTo ─────────────────────────────────────────────────

        public Quantity<U> ConvertTo(U targetUnit)
        {
            if (targetUnit == null)
                throw new ArgumentException("Target unit cannot be null");

            double baseValue = Unit.ConvertToBaseUnit(Value);
            double result    = targetUnit.ConvertFromBaseUnit(baseValue);
            return new Quantity<U>(Round(result), targetUnit);
        }

        // ── Add ───────────────────────────────────────────────────────

        public Quantity<U> Add(Quantity<U> other) => Add(other, Unit);

        public Quantity<U> Add(Quantity<U> other, U targetUnit)
        {
            ValidateArithmeticOperands(other, true, targetUnit);
            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.ADD);
            double result     = targetUnit.ConvertFromBaseUnit(baseResult);
            return new Quantity<U>(Round(result), targetUnit);
        }

        // ── Subtract ──────────────────────────────────────────────────

        public Quantity<U> Subtract(Quantity<U> other) => Subtract(other, Unit);

        public Quantity<U> Subtract(Quantity<U> other, U targetUnit)
        {
            ValidateArithmeticOperands(other, true, targetUnit);
            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.SUBTRACT);
            double result     = targetUnit.ConvertFromBaseUnit(baseResult);
            return new Quantity<U>(Round(result), targetUnit);
        }

        // ── Divide ────────────────────────────────────────────────────

        public double Divide(Quantity<U> other)
        {
            ValidateArithmeticOperands(other, false, default);
            return PerformBaseArithmetic(other, ArithmeticOperation.DIVIDE);
        }

        // ── Equality ──────────────────────────────────────────────────

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (Quantity<U>)obj;
            if (Unit.GetCategory() != other.Unit.GetCategory()) return false;

            double base1 = Unit.ConvertToBaseUnit(Value);
            double base2 = other.Unit.ConvertToBaseUnit(other.Value);
            return Math.Abs(base1 - base2) < 0.0001;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"Quantity({Value}, {Unit.GetUnitName()})";
    }
}
