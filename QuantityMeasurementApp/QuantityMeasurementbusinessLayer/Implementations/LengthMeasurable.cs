using System;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer.Implementations
{
    public class LengthMeasurable : IMeasurable
    {
        private LengthUnit _unit;

        public LengthMeasurable(LengthUnit unit)
        {
            _unit = unit;
        }

        public string GetUnitName()
        {
            if (_unit == LengthUnit.FEET)
            {
                return "FEET";
            }
            if (_unit == LengthUnit.INCHES)
            {
                return "INCHES";
            }
            if (_unit == LengthUnit.YARDS)
            {
                return "YARDS";
            }
            if (_unit == LengthUnit.CENTIMETERS)
            {
                return "CENTIMETERS";
            }
            return "UNKNOWN";
        }

        public string GetCategory()
        {
            return "Length";
        }

        // Returns the factor to convert this unit to the base unit (FEET)
        // FEET = 1.0, INCHES = 1/12, YARDS = 3, CENTIMETERS = 1/30.48
        public double GetConversionFactor()
        {
            if (_unit == LengthUnit.FEET)
            {
                return 1.0;
            }
            if (_unit == LengthUnit.INCHES)
            {
                return 1.0 / 12.0;
            }
            if (_unit == LengthUnit.YARDS)
            {
                return 3.0;
            }
            if (_unit == LengthUnit.CENTIMETERS)
            {
                return 1.0 / 30.48;
            }
            throw new ArgumentException("Unknown LengthUnit: " + _unit);
        }

        // Base unit = FEET
        public double ConvertToBaseUnit(double value)
        {
            if (_unit == LengthUnit.FEET)
            {
                return value;
            }
            if (_unit == LengthUnit.INCHES)
            {
                return value / 12.0;
            }
            if (_unit == LengthUnit.YARDS)
            {
                return value * 3.0;
            }
            if (_unit == LengthUnit.CENTIMETERS)
            {
                return value / 30.48;
            }
            throw new ArgumentException("Unknown LengthUnit: " + _unit);
        }

        public double ConvertFromBaseUnit(double baseValue)
        {
            if (_unit == LengthUnit.FEET)
            {
                return baseValue;
            }
            if (_unit == LengthUnit.INCHES)
            {
                return baseValue * 12.0;
            }
            if (_unit == LengthUnit.YARDS)
            {
                return baseValue / 3.0;
            }
            if (_unit == LengthUnit.CENTIMETERS)
            {
                return baseValue * 30.48;
            }
            throw new ArgumentException("Unknown LengthUnit: " + _unit);
        }

        public void ValidateOperationSupport(string operation)
        {
            // Length supports all arithmetic operations
        }
    }
}