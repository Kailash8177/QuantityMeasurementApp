using System;

namespace QuantityMeasurementApp
{
    public enum LengthUnit
    {
        FEET,
        INCHES,
        YARDS,
        CENTIMETERS
    }

    public static class LengthUnitExtensions
    {
        // Convert value in this unit → base unit (FEET)
        public static double ConvertToBaseUnit(this LengthUnit unit, double value)
        {
            return unit switch
            {
                LengthUnit.FEET => value,
                LengthUnit.INCHES => value / 12.0,
                LengthUnit.YARDS => value * 3.0,
                LengthUnit.CENTIMETERS => value / 30.48,
                _ => throw new ArgumentException("Unsupported unit")
            };
        }

        // Convert value from base unit (FEET) → this unit
        public static double ConvertFromBaseUnit(this LengthUnit unit, double baseValue)
        {
            return unit switch
            {
                LengthUnit.FEET => baseValue,
                LengthUnit.INCHES => baseValue * 12.0,
                LengthUnit.YARDS => baseValue / 3.0,
                LengthUnit.CENTIMETERS => baseValue * 30.48,
                _ => throw new ArgumentException("Unsupported unit")
            };
        }
    }
}