using System;

namespace QuantityMeasurementApp
{
    public enum LengthUnit
    {
        Feet,
        Inch,
        Yard,
        Centimeter
    }

    public static class LengthUnitExtensions
    {
        // Conversion factor relative to base unit (Feet)
        public static double ToFeetFactor(this LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Feet => 1.0,
                LengthUnit.Inch => 1.0 / 12.0,
                LengthUnit.Yard => 3.0,
                LengthUnit.Centimeter => 0.0328084, // 1 cm in feet
                _ => throw new ArgumentException("Unsupported unit")
            };
        }
    }
}