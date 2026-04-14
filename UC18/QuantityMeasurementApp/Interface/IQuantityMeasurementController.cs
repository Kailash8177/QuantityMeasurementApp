using QuantityMeasurementModelLayer.DTOs;

namespace QuantityMeasurementApp.Interface
{
    public interface IQuantityMeasurementController
    {
        void PerformComparison(QuantityDTO dto1, QuantityDTO dto2);
        void PerformConversion(QuantityDTO dto, string targetUnit);
        void PerformAddition(QuantityDTO dto1, QuantityDTO dto2, string targetUnit);
        void PerformSubtraction(QuantityDTO dto1, QuantityDTO dto2, string targetUnit);
        void PerformDivision(QuantityDTO dto1, QuantityDTO dto2);
    }
}
