namespace QuantityMeasurementApp.Core.Interfaces
{
    public interface IMeasurable
    {
        double GetConversionFactor();

        double ConvertToBaseUnit(double value);

        double ConvertFromBaseUnit(double baseValue);

        string GetUnitName();

        // UC14 support check
        bool SupportsArithmeticOperation();

        void ValidateOperationSupport(string operation);
    }
}