using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementbusinessLayer.Implementations;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Repositories;

namespace QuantityMeasurementTests
{
    // ================================================================
    // UC15 Test Suite — 41 test cases matching the spec
    // All tests use the controller as the entry point (same as the app)
    // and verify entity results from the service layer.
    // ================================================================

    [TestClass]
    public class QuantityMeasurementAppTest
    {
        private static IQuantityMeasurementController _controller = null!;
        private static IQuantityMeasurementService _service = null!;
        private static IQuantityMeasurementRepository _repository = null!;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            _repository = QuantityMeasurementCacheRepository.GetInstance();
            _service = new QuantityMeasurementServiceImpl(_repository);

            QuantityMeasurementApp.QuantityMeasurementApp app =
                QuantityMeasurementApp.QuantityMeasurementApp.GetInstance();
            _controller = app.GetController();
        }

        // ============================================================
        // Helper to build QuantityDTO objects cleanly
        // ============================================================

        private static QuantityDTO Dto(double value, string unit, string category)
        {
            return new QuantityDTO(value, unit, category);
        }

        // ============================================================
        // 1. Entity Layer Tests
        // ============================================================

        [TestMethod]
        public void TestQuantityEntity_SingleOperandConstruction()
        {
            QuantityDTO op = Dto(1.0, "FEET", "Length");
            QuantityMeasurementEntity entity =
                new QuantityMeasurementEntity(op, "CONVERT", 12.0, "INCHES");

            Assert.AreEqual("CONVERT", entity.OperationType);
            Assert.AreEqual(12.0, entity.ResultValue, 0.001);
            Assert.AreEqual("INCHES", entity.ResultUnit);
            Assert.IsFalse(entity.HasError);
            Assert.IsFalse(entity.IsComparison);
        }

        [TestMethod]
        public void TestQuantityEntity_BinaryOperandConstruction()
        {
            QuantityDTO op1 = Dto(1.0, "FEET", "Length");
            QuantityDTO op2 = Dto(12.0, "INCHES", "Length");
            QuantityMeasurementEntity entity =
                new QuantityMeasurementEntity(op1, op2, "ADD", 2.0, "FEET");

            Assert.AreEqual("ADD", entity.OperationType);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
            Assert.AreEqual("FEET", entity.ResultUnit);
            Assert.IsNotNull(entity.Operand2);
            Assert.IsFalse(entity.HasError);
        }

        [TestMethod]
        public void TestQuantityEntity_ErrorConstruction()
        {
            QuantityMeasurementEntity entity =
                new QuantityMeasurementEntity("ADD", "Temperature does not support ADD operation.");

            Assert.IsTrue(entity.HasError);
            Assert.AreEqual("ADD", entity.OperationType);
            Assert.IsNotNull(entity.ErrorMessage);
        }

        [TestMethod]
        public void TestQuantityEntity_ToString_Success()
        {
            QuantityDTO op = Dto(1.0, "LITRE", "Volume");
            QuantityMeasurementEntity entity =
                new QuantityMeasurementEntity(op, "CONVERT", 1000.0, "MILLILITRE");

            string result = entity.ToString();
            Assert.IsTrue(result.Contains("CONVERT"));
            Assert.IsTrue(result.Contains("1000"));
        }

        [TestMethod]
        public void TestQuantityEntity_ToString_Error()
        {
            QuantityMeasurementEntity entity =
                new QuantityMeasurementEntity("DIVIDE", "Division by zero");

            string result = entity.ToString();
            Assert.IsTrue(result.Contains("ERROR"));
            Assert.IsTrue(result.Contains("Division by zero"));
        }

        // ============================================================
        // 2. Service Layer — Comparison Tests
        // ============================================================

        [TestMethod]
        public void TestService_CompareEquality_SameUnit_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(1.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsFalse(entity.HasError);
            Assert.IsTrue(entity.IsComparison);
            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestService_CompareEquality_DifferentUnit_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(12.0, "INCHES", "Length");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsFalse(entity.HasError);
            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestService_CompareEquality_CrossCategory_Error()
        {
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(1.0, "LITRE", "Volume");

            try
            {
                _service.Compare(dto1, dto2);
                Assert.Fail("Expected QuantityMeasurementException");
            }
            catch (QuantityMeasurementException ex)
            {
                Assert.IsTrue(ex.Message.Contains("categor") || ex.Message.Contains("mix"));
            }
        }

        // ============================================================
        // 3. Service Layer — Conversion Tests
        // ============================================================

        [TestMethod]
        public void TestService_Convert_Success()
        {
            QuantityDTO dto = Dto(1.0, "LITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Convert(dto, "MILLILITRE");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(1000.0, entity.ResultValue, 0.001);
            Assert.AreEqual("MILLILITRE", entity.ResultUnit);
        }

        [TestMethod]
        public void TestService_Convert_FeetToInches_Success()
        {
            QuantityDTO dto = Dto(1.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Convert(dto, "INCHES");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(12.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Convert_CelsiusToFahrenheit_Success()
        {
            QuantityDTO dto = Dto(100.0, "CELSIUS", "Temperature");

            QuantityMeasurementEntity entity = _service.Convert(dto, "FAHRENHEIT");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(212.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Convert_CelsiusToKelvin_Success()
        {
            QuantityDTO dto = Dto(0.0, "CELSIUS", "Temperature");

            QuantityMeasurementEntity entity = _service.Convert(dto, "KELVIN");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(273.15, entity.ResultValue, 0.001);
        }

        // ============================================================
        // 4. Service Layer — Addition Tests
        // ============================================================

        [TestMethod]
        public void TestService_Add_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(1000.0, "MILLILITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "LITRE");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
            Assert.AreEqual("LITRE", entity.ResultUnit);
        }

        [TestMethod]
        public void TestService_Add_LengthFeetAndInches_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(12.0, "INCHES", "Length");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "FEET");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Add_UnsupportedOperation_Temperature_Error()
        {
            QuantityDTO dto1 = Dto(25.0, "CELSIUS", "Temperature");
            QuantityDTO dto2 = Dto(77.0, "FAHRENHEIT", "Temperature");

            try
            {
                _service.Add(dto1, dto2, "CELSIUS");
                Assert.Fail("Expected QuantityMeasurementException for temperature addition");
            }
            catch (QuantityMeasurementException ex)
            {
                Assert.IsTrue(ex.Message.Contains("Temperature") ||
                              ex.Message.Contains("operation"));
            }
        }

        // ============================================================
        // 5. Service Layer — Subtraction Tests
        // ============================================================

        [TestMethod]
        public void TestService_Subtract_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(500.0, "GRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Subtract(dto1, dto2, "KILOGRAM");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(0.5, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Subtract_VolumeInSameUnit_Success()
        {
            QuantityDTO dto1 = Dto(5.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(2.0, "LITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Subtract(dto1, dto2, "LITRE");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(3.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Subtract_VolumeWithDifferentUnits_Success()
        {
            QuantityDTO dto1 = Dto(2.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(1000.0, "MILLILITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Subtract(dto1, dto2, "LITRE");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(1.0, entity.ResultValue, 0.001);
        }

        // ============================================================
        // 6. Service Layer — Division Tests
        // ============================================================

        [TestMethod]
        public void TestService_Divide_Success()
        {
            QuantityDTO dto1 = Dto(2.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(1.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Divide(dto1, dto2);

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Divide_ByZero_Error()
        {
            QuantityDTO dto1 = Dto(5.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(0.0, "LITRE", "Volume");

            try
            {
                _service.Divide(dto1, dto2);
                Assert.Fail("Expected exception for division by zero");
            }
            catch (QuantityMeasurementException ex)
            {
                Assert.IsTrue(ex.Message.Contains("zero") || ex.Message.Contains("Divide"));
            }
            catch (ArithmeticException)
            {
                // Also acceptable
            }
        }

        [TestMethod]
        public void TestService_Divide_WeightsInSameUnit_Success()
        {
            QuantityDTO dto1 = Dto(4.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(2.0, "KILOGRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Divide(dto1, dto2);

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Divide_WeightsWithDifferentUnits_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(500.0, "GRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Divide(dto1, dto2);

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
        }

        // ============================================================
        // 7. Controller Layer Tests
        // ============================================================

        [TestMethod]
        public void TestController_DemonstrateEquality_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(12.0, "INCHES", "Length");

            // Should not throw — controller handles all exceptions internally
            _controller.PerformComparison(dto1, dto2);
        }

        [TestMethod]
        public void TestController_DemonstrateConversion_Success()
        {
            QuantityDTO dto = Dto(1.0, "KILOGRAM", "Weight");

            _controller.PerformConversion(dto, "GRAM");
        }

        [TestMethod]
        public void TestController_DemonstrateAddition_Success()
        {
            QuantityDTO dto1 = Dto(1.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(1000.0, "MILLILITRE", "Volume");

            _controller.PerformAddition(dto1, dto2, "LITRE");
        }

        [TestMethod]
        public void TestController_DemonstrateAddition_Error()
        {
            // Temperature addition should display an error, not throw
            QuantityDTO dto1 = Dto(25.0, "CELSIUS", "Temperature");
            QuantityDTO dto2 = Dto(77.0, "FAHRENHEIT", "Temperature");

            _controller.PerformAddition(dto1, dto2, "CELSIUS");
            // No exception means controller handled it correctly
        }

        [TestMethod]
        public void TestController_DemonstrateSubtraction_Success()
        {
            QuantityDTO dto1 = Dto(5.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(2.0, "KILOGRAM", "Weight");

            _controller.PerformSubtraction(dto1, dto2, "KILOGRAM");
        }

        [TestMethod]
        public void TestController_DemonstrateDivision_Success()
        {
            QuantityDTO dto1 = Dto(10.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(2.0, "LITRE", "Volume");

            _controller.PerformDivision(dto1, dto2);
        }

        // ============================================================
        // 8. Layer Separation Tests
        // ============================================================

        [TestMethod]
        public void TestLayerSeparation_ServiceIndependence()
        {
            // Service can be used without controller
            IQuantityMeasurementRepository repo = QuantityMeasurementCacheRepository.GetInstance();
            IQuantityMeasurementService service = new QuantityMeasurementServiceImpl(repo);

            QuantityDTO dto = Dto(1.0, "FEET", "Length");
            QuantityMeasurementEntity entity = service.Convert(dto, "INCHES");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(12.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestLayerSeparation_ControllerIndependence()
        {
            // Controller works with any IQuantityMeasurementService implementation
            IQuantityMeasurementRepository repo = QuantityMeasurementCacheRepository.GetInstance();
            IQuantityMeasurementService service = new QuantityMeasurementServiceImpl(repo);
            IQuantityMeasurementController controller =
                new QuantityMeasurementApp.Controllers.QuantityMeasurementController(service);

            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(1.0, "FEET", "Length");

            controller.PerformComparison(dto1, dto2);
        }

        // ============================================================
        // 9. Data Flow Tests
        // ============================================================

        [TestMethod]
        public void TestDataFlow_ControllerToService()
        {
            // DTO flows Controller -> Service -> Entity result returned
            QuantityDTO dto1 = Dto(1.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(1000.0, "GRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "KILOGRAM");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
            Assert.AreEqual("ADD", entity.OperationType);
        }

        [TestMethod]
        public void TestDataFlow_ServiceToController()
        {
            // Service returns entity; controller presents it
            QuantityDTO dto1 = Dto(2.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(1.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Divide(dto1, dto2);

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
            Assert.AreEqual("DIVIDE", entity.OperationType);
        }

        // ============================================================
        // 10. Backward Compatibility Tests (UC1-UC14)
        // ============================================================

        [TestMethod]
        public void TestBackwardCompatibility_LengthFeetEqualsInches()
        {
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(12.0, "INCHES", "Length");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestBackwardCompatibility_LengthYardsEqualsFeet()
        {
            QuantityDTO dto1 = Dto(1.0, "YARDS", "Length");
            QuantityDTO dto2 = Dto(3.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestBackwardCompatibility_WeightKilogramEqualsGrams()
        {
            QuantityDTO dto1 = Dto(1.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(1000.0, "GRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestBackwardCompatibility_VolumeLiterEqualsMilliliters()
        {
            QuantityDTO dto1 = Dto(1.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(1000.0, "MILLILITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestBackwardCompatibility_TemperatureUnitConversion()
        {
            QuantityDTO dto = Dto(100.0, "CELSIUS", "Temperature");

            QuantityMeasurementEntity entity = _service.Convert(dto, "FAHRENHEIT");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(212.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestBackwardCompatibility_TemperatureUnitComparison()
        {
            // 77°C equals 170.6°F
            QuantityDTO dto1 = Dto(77.0, "CELSIUS", "Temperature");
            QuantityDTO dto2 = Dto(170.6, "FAHRENHEIT", "Temperature");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestBackwardCompatibility_AddLengthInSameUnit()
        {
            QuantityDTO dto1 = Dto(2.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(3.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "FEET");

            Assert.AreEqual(5.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestBackwardCompatibility_AddWeightInSameUnit()
        {
            QuantityDTO dto1 = Dto(2.0, "KILOGRAM", "Weight");
            QuantityDTO dto2 = Dto(3.0, "KILOGRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "KILOGRAM");

            Assert.AreEqual(5.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestBackwardCompatibility_AddLengthYardsAndFeet()
        {
            QuantityDTO dto1 = Dto(1.0, "YARDS", "Length");
            QuantityDTO dto2 = Dto(3.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "FEET");

            Assert.AreEqual(6.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestBackwardCompatibility_ConvertLengthFeetToInches()
        {
            QuantityDTO dto = Dto(2.0, "FEET", "Length");

            QuantityMeasurementEntity entity = _service.Convert(dto, "INCHES");

            Assert.AreEqual(24.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestBackwardCompatibility_ConvertWeightKilogramsToGrams()
        {
            QuantityDTO dto = Dto(1.0, "KILOGRAM", "Weight");

            QuantityMeasurementEntity entity = _service.Convert(dto, "GRAM");

            Assert.AreEqual(1000.0, entity.ResultValue, 0.001);
        }

        // ============================================================
        // 11. Validation Tests
        // ============================================================

        [TestMethod]
        public void TestService_NullEntity_Rejection()
        {
            try
            {
               _service.Compare(null!, Dto(1.0, "FEET", "Length"));
                Assert.Fail("Expected exception for null DTO");
            }
            catch (QuantityMeasurementException ex)
            {
                Assert.IsTrue(ex.Message.Contains("null"));
            }
        }

        [TestMethod]
        public void TestController_NullService_Prevention()
        {
            try
            {
                QuantityMeasurementApp.Controllers.QuantityMeasurementController ctrl = 
                new QuantityMeasurementApp.Controllers.QuantityMeasurementController(null!);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void TestService_ValidationConsistency()
        {
            // Same category mismatch error should occur for Add, Subtract, Compare
            QuantityDTO length = Dto(1.0, "FEET", "Length");
            QuantityDTO weight = Dto(1.0, "GRAM", "Weight");

            bool compareThrew = false;
            bool addThrew = false;
            bool subtractThrew = false;

            try { _service.Compare(length, weight); }
            catch (QuantityMeasurementException) { compareThrew = true; }

            try { _service.Add(length, weight, "FEET"); }
            catch (QuantityMeasurementException) { addThrew = true; }

            try { _service.Subtract(length, weight, "FEET"); }
            catch (QuantityMeasurementException) { subtractThrew = true; }

            Assert.IsTrue(compareThrew, "Compare should reject cross-category");
            Assert.IsTrue(addThrew, "Add should reject cross-category");
            Assert.IsTrue(subtractThrew, "Subtract should reject cross-category");
        }

        // ============================================================
        // 12. All Measurement Categories
        // ============================================================

        [TestMethod]
        public void TestService_AllMeasurementCategories()
        {
            // Length
            QuantityMeasurementEntity r1 =
                _service.Convert(Dto(1.0, "FEET", "Length"), "INCHES");
            Assert.AreEqual(12.0, r1.ResultValue, 0.001);

            // Weight
            QuantityMeasurementEntity r2 =
                _service.Convert(Dto(1.0, "KILOGRAM", "Weight"), "GRAM");
            Assert.AreEqual(1000.0, r2.ResultValue, 0.001);

            // Volume
            QuantityMeasurementEntity r3 =
                _service.Convert(Dto(1.0, "LITRE", "Volume"), "MILLILITRE");
            Assert.AreEqual(1000.0, r3.ResultValue, 0.001);

            // Temperature
            QuantityMeasurementEntity r4 =
                _service.Convert(Dto(0.0, "CELSIUS", "Temperature"), "FAHRENHEIT");
            Assert.AreEqual(32.0, r4.ResultValue, 0.001);
        }

        // ============================================================
        // 13. Entity Immutability Test
        // ============================================================

        [TestMethod]
        public void TestEntity_Immutability()
        {
            QuantityDTO op = Dto(1.0, "FEET", "Length");
            QuantityMeasurementEntity entity =
                new QuantityMeasurementEntity(op, "CONVERT", 12.0, "INCHES");

            // Entity properties are get-only — verify values unchanged after creation
            Assert.AreEqual(12.0, entity.ResultValue, 0.001);
            Assert.AreEqual("INCHES", entity.ResultUnit);
            Assert.AreEqual("CONVERT", entity.OperationType);
            Assert.IsFalse(entity.HasError);
        }

        // ============================================================
        // 14. Integration End-to-End Tests
        // ============================================================

        [TestMethod]
        public void TestIntegration_EndToEnd_LengthAddition()
        {
            // Full layer flow: build DTOs -> service -> result
            QuantityDTO dto1 = Dto(1.0, "FEET", "Length");
            QuantityDTO dto2 = Dto(12.0, "INCHES", "Length");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "FEET");

            Assert.IsFalse(entity.HasError);
            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
            Assert.AreEqual("FEET", entity.ResultUnit);
            Assert.AreEqual("ADD", entity.OperationType);
        }

        [TestMethod]
        public void TestIntegration_EndToEnd_TemperatureUnsupported()
        {
            // Full layer flow for an error case
            QuantityDTO dto1 = Dto(0.0, "CELSIUS", "Temperature");
            QuantityDTO dto2 = Dto(32.0, "FAHRENHEIT", "Temperature");

            try
            {
                _service.Add(dto1, dto2, "CELSIUS");
                Assert.Fail("Expected QuantityMeasurementException");
            }
            catch (QuantityMeasurementException)
            {
                // Controller handles this and displays error — no crash
                _controller.PerformAddition(dto1, dto2, "CELSIUS");
            }
        }

        // ============================================================
        // 15. Volume-specific Tests
        // ============================================================

        [TestMethod]
        public void TestVolumeLitersEqualsMilliliters()
        {
            QuantityDTO dto1 = Dto(1.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(1000.0, "MILLILITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Compare(dto1, dto2);

            Assert.IsTrue(entity.ComparisonResult);
        }

        [TestMethod]
        public void TestConvertVolumeToMilliliters()
        {
            QuantityDTO dto = Dto(1.0, "LITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Convert(dto, "MILLILITRE");

            Assert.AreEqual(1000.0, entity.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestAddVolumeLitersAndMilliliters()
        {
            QuantityDTO dto1 = Dto(1.0, "LITRE", "Volume");
            QuantityDTO dto2 = Dto(1000.0, "MILLILITRE", "Volume");

            QuantityMeasurementEntity entity = _service.Add(dto1, dto2, "LITRE");

            Assert.AreEqual(2.0, entity.ResultValue, 0.001);
        }

        // ============================================================
        // 16. Prevent cross-type operations
        // ============================================================

        [TestMethod]
        public void TestPreventCrossTypeAdditionLengthVsWeight()
        {
            QuantityDTO length = Dto(2.0, "FEET", "Length");
            QuantityDTO weight = Dto(10.0, "KILOGRAM", "Weight");

            try
            {
                _service.Add(length, weight, "FEET");
                Assert.Fail("Expected exception for cross-category addition");
            }
            catch (QuantityMeasurementException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void TestPreventCrossTypeConversionLengthToWeight()
        {
            // Category is "Length" but target unit is a weight unit
            QuantityDTO dto = Dto(1.0, "FEET", "Length");

            try
            {
                _service.Convert(dto, "KILOGRAM");
                Assert.Fail("Expected exception for invalid length unit");
            }
            catch (QuantityMeasurementException)
            {
                // Expected
            }
        }
    }
}