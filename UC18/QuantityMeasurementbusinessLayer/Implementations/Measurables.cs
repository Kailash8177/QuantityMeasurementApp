using System;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer.Interfaces
{
    /// <summary>
    /// Contract every measurable unit must satisfy.
    /// Analogous to Java's IMeasurable interface from UC16.
    /// </summary>
    public interface IMeasurable
    {
        double GetConversionFactor();
        double ConvertToBaseUnit(double value);
        double ConvertFromBaseUnit(double baseValue);
        string GetUnitName();
        string GetCategory();

        /// <summary>
        /// Throws InvalidOperationException if the unit's category
        /// does not support <paramref name="operation"/>.
        /// </summary>
        void ValidateOperationSupport(string operation);
    }
}

// ── Concrete implementations ────────────────────────────────────────────────

namespace QuantityMeasurementbusinessLayer.Implementations
{
    using QuantityMeasurementbusinessLayer.Interfaces;

    // ── Length ───────────────────────────────────────────────────────────────
    public class LengthMeasurable : IMeasurable
    {
        private readonly LengthUnit _unit;
        public LengthMeasurable(LengthUnit unit) => _unit = unit;

        public string GetCategory()  => "Length";
        public string GetUnitName()  => _unit.ToString();

        // Base unit = FEET
        public double GetConversionFactor() => _unit switch
        {
            LengthUnit.FEET        => 1.0,
            LengthUnit.INCHES      => 1.0 / 12.0,
            LengthUnit.YARDS       => 3.0,
            LengthUnit.CENTIMETERS => 1.0 / 30.48,
            _ => throw new ArgumentException($"Unknown LengthUnit: {_unit}")
        };

        public double ConvertToBaseUnit(double value) => _unit switch
        {
            LengthUnit.FEET        => value,
            LengthUnit.INCHES      => value / 12.0,
            LengthUnit.YARDS       => value * 3.0,
            LengthUnit.CENTIMETERS => value / 30.48,
            _ => throw new ArgumentException($"Unknown LengthUnit: {_unit}")
        };

        public double ConvertFromBaseUnit(double baseValue) => _unit switch
        {
            LengthUnit.FEET        => baseValue,
            LengthUnit.INCHES      => baseValue * 12.0,
            LengthUnit.YARDS       => baseValue / 3.0,
            LengthUnit.CENTIMETERS => baseValue * 30.48,
            _ => throw new ArgumentException($"Unknown LengthUnit: {_unit}")
        };

        public void ValidateOperationSupport(string operation) { /* all supported */ }
    }

    // ── Weight ───────────────────────────────────────────────────────────────
    public class WeightMeasurable : IMeasurable
    {
        private readonly WeightUnit _unit;
        public WeightMeasurable(WeightUnit unit) => _unit = unit;

        public string GetCategory()  => "Weight";
        public string GetUnitName()  => _unit.ToString();

        public double GetConversionFactor() => _unit switch
        {
            WeightUnit.KILOGRAM => 1.0,
            WeightUnit.GRAM     => 0.001,
            _ => throw new ArgumentException($"Unknown WeightUnit: {_unit}")
        };

        public double ConvertToBaseUnit(double value) => _unit switch
        {
            WeightUnit.KILOGRAM => value,
            WeightUnit.GRAM     => value / 1000.0,
            _ => throw new ArgumentException($"Unknown WeightUnit: {_unit}")
        };

        public double ConvertFromBaseUnit(double baseValue) => _unit switch
        {
            WeightUnit.KILOGRAM => baseValue,
            WeightUnit.GRAM     => baseValue * 1000.0,
            _ => throw new ArgumentException($"Unknown WeightUnit: {_unit}")
        };

        public void ValidateOperationSupport(string operation) { /* all supported */ }
    }

    // ── Volume ───────────────────────────────────────────────────────────────
    public class VolumeMeasurable : IMeasurable
    {
        private readonly VolumeUnit _unit;
        public VolumeMeasurable(VolumeUnit unit) => _unit = unit;

        public string GetCategory()  => "Volume";
        public string GetUnitName()  => _unit.ToString();

        public double GetConversionFactor() => _unit switch
        {
            VolumeUnit.LITRE      => 1.0,
            VolumeUnit.MILLILITRE => 0.001,
            VolumeUnit.GALLON     => 3.78541,
            _ => throw new ArgumentException($"Unknown VolumeUnit: {_unit}")
        };

        public double ConvertToBaseUnit(double value) => _unit switch
        {
            VolumeUnit.LITRE      => value,
            VolumeUnit.MILLILITRE => value * 0.001,
            VolumeUnit.GALLON     => value * 3.78541,
            _ => throw new ArgumentException($"Unknown VolumeUnit: {_unit}")
        };

        public double ConvertFromBaseUnit(double baseValue) => _unit switch
        {
            VolumeUnit.LITRE      => baseValue,
            VolumeUnit.MILLILITRE => baseValue / 0.001,
            VolumeUnit.GALLON     => baseValue / 3.78541,
            _ => throw new ArgumentException($"Unknown VolumeUnit: {_unit}")
        };

        public void ValidateOperationSupport(string operation) { /* all supported */ }
    }

    // ── Temperature ──────────────────────────────────────────────────────────
    public class TemperatureMeasurable : IMeasurable
    {
        private readonly TemperatureUnit _unit;
        public TemperatureMeasurable(TemperatureUnit unit) => _unit = unit;

        public string GetCategory()  => "Temperature";
        public string GetUnitName()  => _unit.ToString();
        public double GetConversionFactor() => 1.0; // offset-based — not linear

        // Base unit = CELSIUS
        public double ConvertToBaseUnit(double value) => _unit switch
        {
            TemperatureUnit.CELSIUS    => value,
            TemperatureUnit.FAHRENHEIT => (value - 32) * 5.0 / 9.0,
            TemperatureUnit.KELVIN     => value - 273.15,
            _ => throw new ArgumentException($"Unknown TemperatureUnit: {_unit}")
        };

        public double ConvertFromBaseUnit(double baseValue) => _unit switch
        {
            TemperatureUnit.CELSIUS    => baseValue,
            TemperatureUnit.FAHRENHEIT => (baseValue * 9.0 / 5.0) + 32,
            TemperatureUnit.KELVIN     => baseValue + 273.15,
            _ => throw new ArgumentException($"Unknown TemperatureUnit: {_unit}")
        };

        /// <summary>Temperature does not support arithmetic operations.</summary>
        public void ValidateOperationSupport(string operation) =>
            throw new InvalidOperationException(
                $"Temperature does not support {operation} operation.");
    }
}
