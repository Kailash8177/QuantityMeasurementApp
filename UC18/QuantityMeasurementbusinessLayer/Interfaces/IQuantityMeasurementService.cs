using System.Collections.Generic;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementbusinessLayer.Interfaces
{
    public interface IQuantityMeasurementService
    {
        QuantityMeasurementEntity Compare(QuantityDTO dto1, QuantityDTO dto2);
        QuantityMeasurementEntity Convert(QuantityDTO dto, string targetUnit);
        QuantityMeasurementEntity Add(QuantityDTO dto1, QuantityDTO dto2, string targetUnit);
        QuantityMeasurementEntity Subtract(QuantityDTO dto1, QuantityDTO dto2, string targetUnit);
        QuantityMeasurementEntity Divide(QuantityDTO dto1, QuantityDTO dto2);

        // UC17-style query APIs for REST history endpoints
        List<QuantityMeasurementEntity> GetHistoryByOperation(string operation);
        List<QuantityMeasurementEntity> GetHistoryByType(string measurementType);
        int GetCountByOperation(string operation);
        List<QuantityMeasurementEntity> GetErroredHistory();
    }
}
