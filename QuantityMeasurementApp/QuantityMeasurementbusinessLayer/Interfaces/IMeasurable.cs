namespace QuantityMeasurementbusinessLayer.Interfaces
{
    public interface IMeasurable
    {
        double GetConversionFactor();
        double ConvertToBaseUnit(double value);
        double ConvertFromBaseUnit(double baseValue);
        string GetUnitName();
        string GetCategory();
        void ValidateOperationSupport(string operation);
    }
}