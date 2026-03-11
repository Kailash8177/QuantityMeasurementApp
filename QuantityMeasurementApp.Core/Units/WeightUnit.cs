namespace QuantityMeasurementApp.Core.Units
{
    public enum WeightUnit
    {
        KILOGRAM,
        GRAM,
        POUND
    }

    public static class WeightUnitExtensions
    {
        public static double ConvertToBaseUnit(this WeightUnit unit, double value)
        {
            return unit switch
            {
                WeightUnit.KILOGRAM => value,
                WeightUnit.GRAM => value / 1000.0,
                WeightUnit.POUND => value * 0.453592,
                _ => throw new ArgumentException("Invalid Weight Unit")
            };
        }

        public static double ConvertFromBaseUnit(this WeightUnit unit, double baseValue)
        {
            return unit switch
            {
                WeightUnit.KILOGRAM => baseValue,
                WeightUnit.GRAM => baseValue * 1000.0,
                WeightUnit.POUND => baseValue / 0.453592,
                _ => throw new ArgumentException("Invalid Weight Unit")
            };
        }
    }
}