using System;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementModelLayer.Enums;


namespace QuantityMeasurementbusinessLayer
{
    // ================================================================
    // Quantity<U> — generic measurement class
    // U must be an IMeasurable (e.g. LengthMeasurable, WeightMeasurable)
    // Holds a value + its unit measurable, and delegates all
    // conversion math to the IMeasurable implementation.
    // ================================================================

    public class Quantity<U> where U : IMeasurable
    {
        private double _value;
        private U _unit;

        public double Value
        {
            get { return _value; }
        }

        public U Unit
        {
            get { return _unit; }
        }

        public Quantity(double value, U unit)
        {
            if (unit == null)
            {
                throw new ArgumentException("Unit cannot be null");
            }
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException("Invalid numeric value");
            }
            _value = value;
            _unit  = unit;
        }

        // ============================================================
        // Validation
        // ============================================================

        private void ValidateArithmeticOperands(Quantity<U> other, bool targetRequired, U targetUnit)
        {
            if (other == null)
            {
                throw new ArgumentException("Other quantity cannot be null");
            }
            if (double.IsNaN(other.Value) || double.IsInfinity(other.Value))
            {
                throw new ArgumentException("Invalid numeric value");
            }
            if (_unit.GetCategory() != other.Unit.GetCategory())
            {
                throw new ArgumentException("Measurement categories must match");
            }
            if (targetRequired && targetUnit == null)
            {
                throw new ArgumentException("Target unit cannot be null");
            }
        }

        // ============================================================
        // Central arithmetic — delegates conversion to IMeasurable
        // ============================================================

        private double PerformBaseArithmetic(Quantity<U> other, ArithmeticOperation operation)
        {
            // Delegates to the IMeasurable implementation — no dynamic needed
            _unit.ValidateOperationSupport(operation.ToString());

            double base1 = _unit.ConvertToBaseUnit(_value);
            double base2 = other.Unit.ConvertToBaseUnit(other.Value);

            if (operation == ArithmeticOperation.ADD)
            {
                return base1 + base2;
            }
            if (operation == ArithmeticOperation.SUBTRACT)
            {
                return base1 - base2;
            }
            if (operation == ArithmeticOperation.DIVIDE)
            {
                if (base2 == 0)
                {
                    throw new ArithmeticException("Division by zero");
                }
                return base1 / base2;
            }
            throw new ArgumentException("Unsupported operation");
        }

        private double Round(double value)
        {
            return Math.Round(value, 2);
        }

        // ============================================================
        // ConvertTo
        // ============================================================

        public Quantity<U> ConvertTo(U targetUnit)
        {
            if (targetUnit == null)
            {
                throw new ArgumentException("Target unit cannot be null");
            }
            double baseValue = _unit.ConvertToBaseUnit(_value);
            double result    = targetUnit.ConvertFromBaseUnit(baseValue);
            return new Quantity<U>(Round(result), targetUnit);
        }

        // ============================================================
        // Add
        // ============================================================

        public Quantity<U> Add(Quantity<U> other)
        {
            return Add(other, _unit);
        }

        public Quantity<U> Add(Quantity<U> other, U targetUnit)
        {
            ValidateArithmeticOperands(other, true, targetUnit);
            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.ADD);
            double result     = targetUnit.ConvertFromBaseUnit(baseResult);
            return new Quantity<U>(Round(result), targetUnit);
        }

        // ============================================================
        // Subtract
        // ============================================================

        public Quantity<U> Subtract(Quantity<U> other)
        {
            return Subtract(other, _unit);
        }

        public Quantity<U> Subtract(Quantity<U> other, U targetUnit)
        {
            ValidateArithmeticOperands(other, true, targetUnit);
            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.SUBTRACT);
            double result     = targetUnit.ConvertFromBaseUnit(baseResult);
            return new Quantity<U>(Round(result), targetUnit);
        }

        // ============================================================
        // Divide
        // ============================================================

        public double Divide(Quantity<U> other)
        {
            ValidateArithmeticOperands(other, false, default(U));
            return PerformBaseArithmetic(other, ArithmeticOperation.DIVIDE);
        }

        // ============================================================
        // Equality
        // ============================================================

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            Quantity<U> other = (Quantity<U>)obj;
            if (_unit.GetCategory() != other.Unit.GetCategory())
            {
                return false;
            }
            double base1 = _unit.ConvertToBaseUnit(_value);
            double base2 = other.Unit.ConvertToBaseUnit(other.Value);
            return Math.Abs(base1 - base2) < 0.0001;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return "Quantity(" + _value + ", " + _unit.GetUnitName() + ")";
        }
    }
}
