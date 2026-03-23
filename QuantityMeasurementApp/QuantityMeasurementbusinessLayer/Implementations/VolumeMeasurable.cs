using System;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer.Implementations
{
    public class VolumeMeasurable : IMeasurable
    {
        private VolumeUnit _unit;

        public VolumeMeasurable(VolumeUnit unit)
        {
            _unit = unit;
        }

        public string GetUnitName()
        {
            if (_unit == VolumeUnit.LITRE)
            {
                return "LITRE";
            }
            if (_unit == VolumeUnit.MILLILITRE)
            {
                return "MILLILITRE";
            }
            if (_unit == VolumeUnit.GALLON)
            {
                return "GALLON";
            }
            return "UNKNOWN";
        }

        public string GetCategory()
        {
            return "Volume";
        }

        // Returns the factor to convert this unit to the base unit (LITRE)
        // LITRE = 1.0, MILLILITRE = 0.001, GALLON = 3.78541
        public double GetConversionFactor()
        {
            if (_unit == VolumeUnit.LITRE)
            {
                return 1.0;
            }
            if (_unit == VolumeUnit.MILLILITRE)
            {
                return 0.001;
            }
            if (_unit == VolumeUnit.GALLON)
            {
                return 3.78541;
            }
            throw new ArgumentException("Unknown VolumeUnit: " + _unit);
        }

        // Base unit = LITRE
        public double ConvertToBaseUnit(double value)
        {
            if (_unit == VolumeUnit.LITRE)
            {
                return value;
            }
            if (_unit == VolumeUnit.MILLILITRE)
            {
                return value * 0.001;
            }
            if (_unit == VolumeUnit.GALLON)
            {
                return value * 3.78541;
            }
            throw new ArgumentException("Unknown VolumeUnit: " + _unit);
        }

        public double ConvertFromBaseUnit(double baseValue)
        {
            if (_unit == VolumeUnit.LITRE)
            {
                return baseValue;
            }
            if (_unit == VolumeUnit.MILLILITRE)
            {
                return baseValue / 0.001;
            }
            if (_unit == VolumeUnit.GALLON)
            {
                return baseValue / 3.78541;
            }
            throw new ArgumentException("Unknown VolumeUnit: " + _unit);
        }

        public void ValidateOperationSupport(string operation)
        {
            // Volume supports all arithmetic operations
        }
    }
}