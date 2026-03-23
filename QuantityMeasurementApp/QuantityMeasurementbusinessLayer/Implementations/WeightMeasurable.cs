using System;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer.Implementations
{
    public class WeightMeasurable : IMeasurable
    {
        private WeightUnit _unit;

        public WeightMeasurable(WeightUnit unit)
        {
            _unit = unit;
        }

        public string GetUnitName()
        {
            if (_unit == WeightUnit.KILOGRAM)
            {
                return "KILOGRAM";
            }
            if (_unit == WeightUnit.GRAM)
            {
                return "GRAM";
            }
            return "UNKNOWN";
        }

        public string GetCategory()
        {
            return "Weight";
        }

        // Returns the factor to convert this unit to the base unit (KILOGRAM)
        // KILOGRAM = 1.0, GRAM = 0.001
        public double GetConversionFactor()
        {
            if (_unit == WeightUnit.KILOGRAM)
            {
                return 1.0;
            }
            if (_unit == WeightUnit.GRAM)
            {
                return 0.001;
            }
            throw new ArgumentException("Unknown WeightUnit: " + _unit);
        }

        // Base unit = KILOGRAM
        public double ConvertToBaseUnit(double value)
        {
            if (_unit == WeightUnit.KILOGRAM)
            {
                return value;
            }
            if (_unit == WeightUnit.GRAM)
            {
                return value / 1000.0;
            }
            throw new ArgumentException("Unknown WeightUnit: " + _unit);
        }

        public double ConvertFromBaseUnit(double baseValue)
        {
            if (_unit == WeightUnit.KILOGRAM)
            {
                return baseValue;
            }
            if (_unit == WeightUnit.GRAM)
            {
                return baseValue * 1000.0;
            }
            throw new ArgumentException("Unknown WeightUnit: " + _unit);
        }

        public void ValidateOperationSupport(string operation)
        {
            // Weight supports all arithmetic operations
        }
    }
}