using System;

namespace QuantityMeasurementApp.Core.Units
{
    public enum LengthUnit
    {
        FEET,
        INCHES
    }

    public static class LengthUnitExtensions
    {
        public static double ConvertToBaseUnit(this LengthUnit unit, double value)
        {
            return unit switch
            {
                LengthUnit.FEET => value,
                LengthUnit.INCHES => value / 12,
                _ => throw new ArgumentException("Invalid Length Unit")
            };
        }

        public static double ConvertFromBaseUnit(this LengthUnit unit, double baseValue)
        {
            return unit switch
            {
                LengthUnit.FEET => baseValue,
                LengthUnit.INCHES => baseValue * 12,
                _ => throw new ArgumentException("Invalid Length Unit")
            };
        }

        public static void ValidateOperationSupport(this LengthUnit unit, string operation)
        {
            // Length supports all arithmetic operations
        }
    }
}