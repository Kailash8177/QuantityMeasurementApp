using System;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementApp.Controllers
{
    public class QuantityMeasurementController : IQuantityMeasurementController
    {
        private IQuantityMeasurementService _service;

        public QuantityMeasurementController(IQuantityMeasurementService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service cannot be null");
            }
            _service = service;
        }

        public void PerformComparison(QuantityDTO dto1, QuantityDTO dto2)
        {
            try
            {
                QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);
                DisplayResult(entity);
            }
            catch (QuantityMeasurementException ex)
            {
                DisplayError("PerformComparison", ex.Message);
            }
        }

        public void PerformConversion(QuantityDTO dto, string targetUnit)
        {
            try
            {
                QuantityMeasurementEntity entity = _service.Convert(dto, targetUnit);
                DisplayResult(entity);
            }
            catch (QuantityMeasurementException ex)
            {
                DisplayError("PerformConversion", ex.Message);
            }
        }

        public void PerformAddition(QuantityDTO dto1, QuantityDTO dto2, string targetUnit)
        {
            try
            {
                QuantityMeasurementEntity entity = _service.Add(dto1, dto2, targetUnit);
                DisplayResult(entity);
            }
            catch (QuantityMeasurementException ex)
            {
                DisplayError("PerformAddition", ex.Message);
            }
        }

        public void PerformSubtraction(QuantityDTO dto1, QuantityDTO dto2, string targetUnit)
        {
            try
            {
                QuantityMeasurementEntity entity = _service.Subtract(dto1, dto2, targetUnit);
                DisplayResult(entity);
            }
            catch (QuantityMeasurementException ex)
            {
                DisplayError("PerformSubtraction", ex.Message);
            }
        }

        public void PerformDivision(QuantityDTO dto1, QuantityDTO dto2)
        {
            try
            {
                QuantityMeasurementEntity entity = _service.Divide(dto1, dto2);
                DisplayResult(entity);
            }
            catch (QuantityMeasurementException ex)
            {
                DisplayError("PerformDivision", ex.Message);
            }
        }

        private static void DisplayResult(QuantityMeasurementEntity entity)
        {
            if (entity.HasError)
            {
                Console.WriteLine("  [ERROR]  " + entity.ErrorMessage);
            }
            else
            {
                Console.WriteLine("  [OK]     " + entity.ToString());
            }
        }

        private static void DisplayError(string method, string message)
        {
            Console.WriteLine("  [ERROR]  " + method + ": " + message);
        }
    }
}
