using System;

namespace QuantityMeasurementApp.Core.Units
{
    public enum VolumeUnit
    {
        LITRE,
        MILLILITRE,
        GALLON
    }

    public static class VolumeUnitExtensions
    {
        public static double GetConversionFactor(this VolumeUnit unit)
        {
            return unit switch
            {
                VolumeUnit.LITRE => 1.0,
                VolumeUnit.MILLILITRE => 0.001,
                VolumeUnit.GALLON => 3.78541,
                _ => throw new ArgumentException("Invalid volume unit")
            };
        }

        public static double ConvertToBaseUnit(this VolumeUnit unit, double value)
        {
            return value * unit.GetConversionFactor();
        }

        public static double ConvertFromBaseUnit(this VolumeUnit unit, double baseValue)
        {
            return baseValue / unit.GetConversionFactor();
        }

        public static string GetUnitName(this VolumeUnit unit)
        {
            return unit switch
            {
                VolumeUnit.LITRE => "Litre",
                VolumeUnit.MILLILITRE => "Millilitre",
                VolumeUnit.GALLON => "Gallon",
                _ => "Unknown"
            };
        }
    }
}