using System;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementApp.Controllers
{
    public class QuantityMeasurementController : IQuantityMeasurementController
    {
        private readonly IQuantityMeasurementService _service;

        public QuantityMeasurementController(IQuantityMeasurementService service)
        {
            _service = service
                ?? throw new ArgumentNullException(nameof(service), "Service cannot be null");
            Console.WriteLine($"[Controller] Initialised with service: {service.GetType().Name}");
        }

        public void PerformComparison(QuantityDTO dto1, QuantityDTO dto2)
        {
            try   { DisplayResult(_service.Compare(dto1, dto2)); }
            catch (QuantityMeasurementException ex) { DisplayError("PerformComparison", ex.Message); }
        }

        public void PerformConversion(QuantityDTO dto, string targetUnit)
        {
            try   { DisplayResult(_service.Convert(dto, targetUnit)); }
            catch (QuantityMeasurementException ex) { DisplayError("PerformConversion", ex.Message); }
        }

        public void PerformAddition(QuantityDTO dto1, QuantityDTO dto2, string targetUnit)
        {
            try   { DisplayResult(_service.Add(dto1, dto2, targetUnit)); }
            catch (QuantityMeasurementException ex) { DisplayError("PerformAddition", ex.Message); }
        }

        public void PerformSubtraction(QuantityDTO dto1, QuantityDTO dto2, string targetUnit)
        {
            try   { DisplayResult(_service.Subtract(dto1, dto2, targetUnit)); }
            catch (QuantityMeasurementException ex) { DisplayError("PerformSubtraction", ex.Message); }
        }

        public void PerformDivision(QuantityDTO dto1, QuantityDTO dto2)
        {
            try   { DisplayResult(_service.Divide(dto1, dto2)); }
            catch (QuantityMeasurementException ex) { DisplayError("PerformDivision", ex.Message); }
        }

        private static void DisplayResult(QuantityMeasurementEntity entity)
        {
            if (entity.HasError)
                Console.WriteLine($"  [ERROR]  {entity.ErrorMessage}");
            else
                Console.WriteLine($"  [OK]     {entity}");
        }

        private static void DisplayError(string method, string message) =>
            Console.WriteLine($"  [ERROR]  {method}: {message}");
    }
}
