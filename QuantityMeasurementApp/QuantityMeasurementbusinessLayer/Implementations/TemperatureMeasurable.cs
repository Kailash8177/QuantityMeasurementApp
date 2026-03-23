using System;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer.Implementations
{
    public class TemperatureMeasurable : IMeasurable
    {
        private TemperatureUnit _unit;

        public TemperatureMeasurable(TemperatureUnit unit)
        {
            _unit = unit;
        }

        public string GetUnitName()
        {
            if (_unit == TemperatureUnit.CELSIUS)
            {
                return "CELSIUS";
            }
            if (_unit == TemperatureUnit.FAHRENHEIT)
            {
                return "FAHRENHEIT";
            }
            if (_unit == TemperatureUnit.KELVIN)
            {
                return "KELVIN";
            }
            return "UNKNOWN";
        }

        public string GetCategory()
        {
            return "Temperature";
        }

        // Temperature uses offset-based conversion, not a simple multiplication factor.
        // GetConversionFactor returns 1.0 as a placeholder.
        // All real conversions are handled by ConvertToBaseUnit / ConvertFromBaseUnit.
        public double GetConversionFactor()
        {
            return 1.0;
        }

        // Base unit = CELSIUS
        // FAHRENHEIT : C = (F - 32) * 5 / 9
        // KELVIN     : C = K - 273.15
        public double ConvertToBaseUnit(double value)
        {
            if (_unit == TemperatureUnit.CELSIUS)
            {
                return value;
            }
            if (_unit == TemperatureUnit.FAHRENHEIT)
            {
                return (value - 32) * 5.0 / 9.0;
            }
            if (_unit == TemperatureUnit.KELVIN)
            {
                return value - 273.15;
            }
            throw new ArgumentException("Unknown TemperatureUnit: " + _unit);
        }

        // Base unit = CELSIUS
        // FAHRENHEIT : F = (C * 9 / 5) + 32
        // KELVIN     : K = C + 273.15
        public double ConvertFromBaseUnit(double baseValue)
        {
            if (_unit == TemperatureUnit.CELSIUS)
            {
                return baseValue;
            }
            if (_unit == TemperatureUnit.FAHRENHEIT)
            {
                return (baseValue * 9.0 / 5.0) + 32;
            }
            if (_unit == TemperatureUnit.KELVIN)
            {
                return baseValue + 273.15;
            }
            throw new ArgumentException("Unknown TemperatureUnit: " + _unit);
        }

        public void ValidateOperationSupport(string operation)
        {
            throw new InvalidOperationException(
                "Temperature does not support " + operation + " operation.");
        }
    }
}