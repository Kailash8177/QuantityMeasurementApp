using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementbusinessLayer.Implementations;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementModelLayer.Enums;
using QuantityMeasurementRepositoryLayer.Repositories;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Util;

namespace QuantityMeasurementApp.Tests
{
    // ================================================================
    // UC16 Test Suite — mirrors the 35 test cases specified in the doc
    // Uses MSTest + QuantityMeasurementCacheRepository for offline tests
    // Integration tests that need SQL Server are clearly marked.
    // ================================================================

    // ── 1. Entity Layer ──────────────────────────────────────────────

    [TestClass]
    public class QuantityMeasurementEntityTest
    {
        [TestMethod]
        public void TestEntity_SingleOperandConstruction()
        {
            var op = new QuantityDTO(1.0, "FEET", "Length");
            var e  = new QuantityMeasurementEntity(op, "CONVERT", 12.0, "INCHES");

            Assert.AreEqual("CONVERT", e.OperationType);
            Assert.AreEqual(12.0,      e.ResultValue, 0.001);
            Assert.AreEqual("INCHES",  e.ResultUnit);
            Assert.IsFalse(e.HasError);
            Assert.IsFalse(e.IsComparison);
        }

        [TestMethod]
        public void TestEntity_BinaryOperandConstruction()
        {
            var op1 = new QuantityDTO(1.0,  "FEET",   "Length");
            var op2 = new QuantityDTO(12.0, "INCHES", "Length");
            var e   = new QuantityMeasurementEntity(op1, op2, "ADD", 2.0, "FEET");

            Assert.AreEqual("ADD",  e.OperationType);
            Assert.AreEqual(2.0,    e.ResultValue, 0.001);
            Assert.AreEqual("FEET", e.ResultUnit);
            Assert.IsNotNull(e.Operand2);
            Assert.IsFalse(e.HasError);
        }

        [TestMethod]
        public void TestEntity_ErrorConstruction()
        {
            var e = new QuantityMeasurementEntity("ADD", "Temperature does not support ADD.");

            Assert.IsTrue(e.HasError);
            Assert.AreEqual("ADD", e.OperationType);
            Assert.IsNotNull(e.ErrorMessage);
        }

        [TestMethod]
        public void TestEntity_ComparisonConstruction()
        {
            var op1 = new QuantityDTO(1.0, "FEET",   "Length");
            var op2 = new QuantityDTO(12,  "INCHES", "Length");
            var e   = new QuantityMeasurementEntity(op1, op2, true);

            Assert.IsTrue(e.IsComparison);
            Assert.IsTrue(e.ComparisonResult);
            Assert.AreEqual("COMPARE", e.OperationType);
            Assert.IsFalse(e.HasError);
        }

        [TestMethod]
        public void TestEntity_ToString_Success()
        {
            var op = new QuantityDTO(1.0, "LITRE", "Volume");
            var e  = new QuantityMeasurementEntity(op, "CONVERT", 1000.0, "MILLILITRE");

            string result = e.ToString();
            Assert.IsTrue(result.Contains("CONVERT"));
            Assert.IsTrue(result.Contains("1000"));
        }

        [TestMethod]
        public void TestEntity_ToString_Error()
        {
            var e = new QuantityMeasurementEntity("DIVIDE", "Division by zero");
            Assert.IsTrue(e.ToString().Contains("[ERROR]"));
        }
    }

    // ── 2. DTO Layer ─────────────────────────────────────────────────

    [TestClass]
    public class QuantityDTOTest
    {
        [TestMethod]
        public void TestDTO_Construction()
        {
            var dto = new QuantityDTO(5.0, "KILOGRAM", "Weight");
            Assert.AreEqual(5.0,        dto.Value);
            Assert.AreEqual("KILOGRAM", dto.Unit);
            Assert.AreEqual("Weight",   dto.Category);
        }

        [TestMethod]
        public void TestDTO_DefaultConstructor()
        {
            var dto = new QuantityDTO();
            Assert.AreEqual(0.0,  dto.Value);
            Assert.IsNull(dto.Unit);
            Assert.IsNull(dto.Category);
        }

        [TestMethod]
        public void TestDTO_ToString()
        {
            var dto = new QuantityDTO(1.0, "FEET", "Length");
            Assert.IsTrue(dto.ToString().Contains("FEET"));
            Assert.IsTrue(dto.ToString().Contains("Length"));
        }
    }

    // ── 3. IMeasurable implementations ───────────────────────────────

    [TestClass]
    public class MeasurableTest
    {
        [TestMethod]
        public void TestLength_FeetToInches()
        {
            var feet   = new LengthMeasurable(LengthUnit.FEET);
            var inches = new LengthMeasurable(LengthUnit.INCHES);
            double baseVal = feet.ConvertToBaseUnit(1.0);          // 1 ft → 1 ft
            double result  = inches.ConvertFromBaseUnit(baseVal);  // 1 ft → 12 in
            Assert.AreEqual(12.0, result, 0.001);
        }

        [TestMethod]
        public void TestLength_YardsToFeet()
        {
            var yards = new LengthMeasurable(LengthUnit.YARDS);
            var feet  = new LengthMeasurable(LengthUnit.FEET);
            double baseVal = yards.ConvertToBaseUnit(1.0);
            double result  = feet.ConvertFromBaseUnit(baseVal);
            Assert.AreEqual(3.0, result, 0.001);
        }

        [TestMethod]
        public void TestWeight_KilogramToGram()
        {
            var kg   = new WeightMeasurable(WeightUnit.KILOGRAM);
            var gram = new WeightMeasurable(WeightUnit.GRAM);
            double baseVal = kg.ConvertToBaseUnit(1.0);
            double result  = gram.ConvertFromBaseUnit(baseVal);
            Assert.AreEqual(1000.0, result, 0.001);
        }

        [TestMethod]
        public void TestVolume_LitreToMillilitre()
        {
            var litre = new VolumeMeasurable(VolumeUnit.LITRE);
            var ml    = new VolumeMeasurable(VolumeUnit.MILLILITRE);
            double baseVal = litre.ConvertToBaseUnit(1.0);
            double result  = ml.ConvertFromBaseUnit(baseVal);
            Assert.AreEqual(1000.0, result, 0.001);
        }

        [TestMethod]
        public void TestTemperature_CelsiusToFahrenheit()
        {
            var celsius    = new TemperatureMeasurable(TemperatureUnit.CELSIUS);
            var fahrenheit = new TemperatureMeasurable(TemperatureUnit.FAHRENHEIT);
            double baseVal = celsius.ConvertToBaseUnit(0.0);
            double result  = fahrenheit.ConvertFromBaseUnit(baseVal);
            Assert.AreEqual(32.0, result, 0.001);
        }

        [TestMethod]
        public void TestTemperature_ValidateOperationSupport_Throws()
        {
            var temp = new TemperatureMeasurable(TemperatureUnit.CELSIUS);
            Assert.ThrowsException<InvalidOperationException>(
                () => temp.ValidateOperationSupport("ADD"));
        }

        [TestMethod]
        public void TestLength_GetCategory()
        {
            var m = new LengthMeasurable(LengthUnit.FEET);
            Assert.AreEqual("Length", m.GetCategory());
        }
    }

    // ── 4. Quantity<U> generic class ─────────────────────────────────

    [TestClass]
    public class QuantityGenericTest
    {
        [TestMethod]
        public void TestQuantity_FeetEqualsInches()
        {
            var q1 = new Quantity<LengthMeasurable>(1.0, new LengthMeasurable(LengthUnit.FEET));
            var q2 = new Quantity<LengthMeasurable>(12.0, new LengthMeasurable(LengthUnit.INCHES));
            Assert.IsTrue(q1.Equals(q2));
        }

        [TestMethod]
        public void TestQuantity_FeetNotEqualsYards()
        {
            var q1 = new Quantity<LengthMeasurable>(1.0, new LengthMeasurable(LengthUnit.FEET));
            var q2 = new Quantity<LengthMeasurable>(1.0, new LengthMeasurable(LengthUnit.YARDS));
            Assert.IsFalse(q1.Equals(q2));
        }

        [TestMethod]
        public void TestQuantity_ConvertTo_FeetToInches()
        {
            var q      = new Quantity<LengthMeasurable>(1.0, new LengthMeasurable(LengthUnit.FEET));
            var result = q.ConvertTo(new LengthMeasurable(LengthUnit.INCHES));
            Assert.AreEqual(12.0, result.Value, 0.001);
        }

        [TestMethod]
        public void TestQuantity_Add_FeetAndInches()
        {
            var q1 = new Quantity<LengthMeasurable>(2.0, new LengthMeasurable(LengthUnit.FEET));
            var q2 = new Quantity<LengthMeasurable>(24.0, new LengthMeasurable(LengthUnit.INCHES));
            var r  = q1.Add(q2, new LengthMeasurable(LengthUnit.FEET));
            Assert.AreEqual(4.0, r.Value, 0.001);
        }

        [TestMethod]
        public void TestQuantity_Subtract_FeetAndInches()
        {
            var q1 = new Quantity<LengthMeasurable>(2.0, new LengthMeasurable(LengthUnit.FEET));
            var q2 = new Quantity<LengthMeasurable>(12.0, new LengthMeasurable(LengthUnit.INCHES));
            var r  = q1.Subtract(q2, new LengthMeasurable(LengthUnit.FEET));
            Assert.AreEqual(1.0, r.Value, 0.001);
        }

        [TestMethod]
        public void TestQuantity_Divide_FeetByFeet()
        {
            var q1 = new Quantity<LengthMeasurable>(4.0, new LengthMeasurable(LengthUnit.FEET));
            var q2 = new Quantity<LengthMeasurable>(2.0, new LengthMeasurable(LengthUnit.FEET));
            Assert.AreEqual(2.0, q1.Divide(q2), 0.001);
        }

        [TestMethod]
        public void TestQuantity_Divide_ByZero_Throws()
        {
            var q1 = new Quantity<LengthMeasurable>(4.0, new LengthMeasurable(LengthUnit.FEET));
            var q2 = new Quantity<LengthMeasurable>(0.0, new LengthMeasurable(LengthUnit.FEET));
            Assert.ThrowsException<ArithmeticException>(() => q1.Divide(q2));
        }

        [TestMethod]
        public void TestQuantity_NullUnit_Throws()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new Quantity<LengthMeasurable>(1.0, null));
        }

        [TestMethod]
        public void TestQuantity_TemperatureAdd_Throws()
        {
            var q1 = new Quantity<TemperatureMeasurable>(0.0, new TemperatureMeasurable(TemperatureUnit.CELSIUS));
            var q2 = new Quantity<TemperatureMeasurable>(32.0, new TemperatureMeasurable(TemperatureUnit.FAHRENHEIT));
            Assert.ThrowsException<InvalidOperationException>(() => q1.Add(q2));
        }
    }

    // ── 5. Exception hierarchy ────────────────────────────────────────

    [TestClass]
    public class ExceptionTest
    {
        [TestMethod]
        public void TestDatabaseException_IsQuantityMeasurementException()
        {
            var ex = new DatabaseException("test");
            Assert.IsInstanceOfType(ex, typeof(QuantityMeasurementException));
        }

        [TestMethod]
        public void TestDatabaseException_ConnectionFailed()
        {
            var cause = new Exception("timeout");
            var ex    = DatabaseException.ConnectionFailed("DB unreachable", cause);
            Assert.IsTrue(ex.Message.Contains("Database connection failed"));
            Assert.AreEqual(cause, ex.InnerException);
        }

        [TestMethod]
        public void TestDatabaseException_QueryFailed()
        {
            var ex = DatabaseException.QueryFailed("SELECT *", new Exception("syntax error"));
            Assert.IsTrue(ex.Message.Contains("Query execution failed"));
        }

        [TestMethod]
        public void TestDatabaseException_TransactionFailed()
        {
            var ex = DatabaseException.TransactionFailed("INSERT", new Exception("lock timeout"));
            Assert.IsTrue(ex.Message.Contains("Transaction failed during INSERT"));
        }
    }

    // ── 6. Cache Repository ───────────────────────────────────────────

    [TestClass]
    public class QuantityMeasurementCacheRepositoryTest
    {
        private QuantityMeasurementCacheRepository _repository;

        [TestInitialize]
        public void SetUp()
        {
            _repository = QuantityMeasurementCacheRepository.GetInstance();
            _repository.DeleteAll();
        }

        [TestCleanup]
        public void TearDown() => _repository.DeleteAll();

        private static QuantityMeasurementEntity CreateTestEntity(double value = 1.0) =>
            new QuantityMeasurementEntity(
                new QuantityDTO(value, "FEET", "Length"),
                "CONVERT", value * 12, "INCHES");

        [TestMethod]
        public void TestCacheRepo_SaveEntity()
        {
            var e = CreateTestEntity();
            _repository.Save(e);
            Assert.IsTrue(e.Id > 0);
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestCacheRepo_GetAllMeasurements()
        {
            _repository.Save(CreateTestEntity(1.0));
            _repository.Save(CreateTestEntity(2.0));
            var all = _repository.GetAllMeasurements();
            Assert.AreEqual(2, all.Count);
        }

        [TestMethod]
        public void TestCacheRepo_GetMeasurementsByOperation()
        {
            _repository.Save(CreateTestEntity());
            var results = _repository.GetMeasurementsByOperation("CONVERT");
            Assert.IsTrue(results.Count >= 1);
        }

        [TestMethod]
        public void TestCacheRepo_GetMeasurementsByType()
        {
            _repository.Save(CreateTestEntity());
            var results = _repository.GetMeasurementsByType("Length");
            Assert.IsTrue(results.Count >= 1);
        }

        [TestMethod]
        public void TestCacheRepo_GetTotalCount()
        {
            _repository.Save(CreateTestEntity());
            _repository.Save(CreateTestEntity());
            Assert.AreEqual(2, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestCacheRepo_DeleteAll()
        {
            _repository.Save(CreateTestEntity());
            _repository.DeleteAll();
            Assert.AreEqual(0, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestCacheRepo_Delete_SingleEntity()
        {
            var e = CreateTestEntity();
            _repository.Save(e);
            bool deleted = _repository.Delete(e.Id);
            Assert.IsTrue(deleted);
            Assert.AreEqual(0, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestCacheRepo_FindById()
        {
            var e = CreateTestEntity();
            _repository.Save(e);
            var found = _repository.FindById(e.Id);
            Assert.IsNotNull(found);
            Assert.AreEqual(e.Id, found.Id);
        }

        [TestMethod]
        public void TestCacheRepo_GetPoolStatistics_ReturnsString()
        {
            string stats = _repository.GetPoolStatistics();
            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.Length > 0);
        }
    }

    // ── 7. Service layer ─────────────────────────────────────────────

    [TestClass]
    public class QuantityMeasurementServiceTest
    {
        private QuantityMeasurementServiceImpl _service;
        private QuantityMeasurementCacheRepository _repository;

        [TestInitialize]
        public void SetUp()
        {
            _repository = QuantityMeasurementCacheRepository.GetInstance();
            _repository.DeleteAll();
            _service = new QuantityMeasurementServiceImpl(_repository);
        }

        [TestCleanup]
        public void TearDown() => _repository.DeleteAll();

        private static QuantityDTO Dto(double v, string u, string c) =>
            new QuantityDTO(v, u, c);

        // ── Compare ───────────────────────────────────────────────────

        [TestMethod]
        public void TestService_Compare_FeetEqualsInches_True()
        {
            var e = _service.Compare(Dto(1, "FEET", "Length"), Dto(12, "INCHES", "Length"));
            Assert.IsTrue(e.ComparisonResult);
            Assert.IsFalse(e.HasError);
        }

        [TestMethod]
        public void TestService_Compare_FeetNotEqualsYards_False()
        {
            var e = _service.Compare(Dto(1, "FEET", "Length"), Dto(1, "YARDS", "Length"));
            Assert.IsFalse(e.ComparisonResult);
        }

        [TestMethod]
        public void TestService_Compare_KilogramEqualsGram()
        {
            var e = _service.Compare(Dto(1, "KILOGRAM", "Weight"), Dto(1000, "GRAM", "Weight"));
            Assert.IsTrue(e.ComparisonResult);
        }

        [TestMethod]
        public void TestService_Compare_CelsiusEqualsKelvin()
        {
            var e = _service.Compare(Dto(0, "CELSIUS", "Temperature"), Dto(273.15, "KELVIN", "Temperature"));
            Assert.IsTrue(e.ComparisonResult);
        }

        [TestMethod]
        public void TestService_Compare_DifferentCategories_Throws()
        {
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Compare(Dto(1, "FEET", "Length"), Dto(1, "KILOGRAM", "Weight")));
        }

        // ── Convert ───────────────────────────────────────────────────

        [TestMethod]
        public void TestService_Convert_FeetToInches()
        {
            var e = _service.Convert(Dto(1, "FEET", "Length"), "INCHES");
            Assert.AreEqual(12.0, e.ResultValue, 0.001);
            Assert.AreEqual("INCHES", e.ResultUnit);
        }

        [TestMethod]
        public void TestService_Convert_CelsiusToFahrenheit()
        {
            var e = _service.Convert(Dto(0, "CELSIUS", "Temperature"), "FAHRENHEIT");
            Assert.AreEqual(32.0, e.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Convert_KelvinToCelsius()
        {
            var e = _service.Convert(Dto(273.15, "KELVIN", "Temperature"), "CELSIUS");
            Assert.AreEqual(0.0, e.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Convert_LitreToMillilitre()
        {
            var e = _service.Convert(Dto(1, "LITRE", "Volume"), "MILLILITRE");
            Assert.AreEqual(1000.0, e.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Convert_GallonToLitre()
        {
            var e = _service.Convert(Dto(1, "GALLON", "Volume"), "LITRE");
            Assert.AreEqual(3.79, e.ResultValue, 0.01);
        }

        [TestMethod]
        public void TestService_Convert_EmptyTargetUnit_Throws()
        {
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Convert(Dto(1, "FEET", "Length"), ""));
        }

        // ── Add ───────────────────────────────────────────────────────

        [TestMethod]
        public void TestService_Add_FeetAndInches()
        {
            var e = _service.Add(Dto(2, "FEET", "Length"), Dto(24, "INCHES", "Length"), "FEET");
            Assert.AreEqual(4.0, e.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Add_KilogramsAndGrams()
        {
            var e = _service.Add(Dto(1, "KILOGRAM", "Weight"), Dto(500, "GRAM", "Weight"), "GRAM");
            Assert.AreEqual(1500.0, e.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Add_Temperature_Throws()
        {
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Add(Dto(0, "CELSIUS", "Temperature"), Dto(32, "FAHRENHEIT", "Temperature"), "CELSIUS"));
        }

        // ── Subtract ──────────────────────────────────────────────────

        [TestMethod]
        public void TestService_Subtract_FeetAndInches()
        {
            var e = _service.Subtract(Dto(2, "FEET", "Length"), Dto(12, "INCHES", "Length"), "FEET");
            Assert.AreEqual(1.0, e.ResultValue, 0.001);
        }

        // ── Divide ────────────────────────────────────────────────────

        [TestMethod]
        public void TestService_Divide_FeetByFeet()
        {
            var e = _service.Divide(Dto(4, "FEET", "Length"), Dto(2, "FEET", "Length"));
            Assert.AreEqual(2.0, e.ResultValue, 0.001);
        }

        [TestMethod]
        public void TestService_Divide_ByZero_Throws()
        {
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Divide(Dto(4, "FEET", "Length"), Dto(0, "FEET", "Length")));
        }

        [TestMethod]
        public void TestService_NullDTO_Throws()
        {
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Compare(null, Dto(1, "FEET", "Length")));
        }

        // ── Persistence verification ───────────────────────────────────

        [TestMethod]
        public void TestService_OperationPersisted_ToRepository()
        {
            _service.Convert(Dto(1, "FEET", "Length"), "INCHES");
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestService_MultipleOperationsPersisted()
        {
            _service.Compare(Dto(1, "FEET", "Length"), Dto(12, "INCHES", "Length"));
            _service.Convert(Dto(0, "CELSIUS", "Temperature"), "FAHRENHEIT");
            _service.Add(Dto(1, "KILOGRAM", "Weight"), Dto(500, "GRAM", "Weight"), "GRAM");
            Assert.AreEqual(3, _repository.GetTotalCount());
        }
    }

    // ── 8. Controller layer ───────────────────────────────────────────

    [TestClass]
    public class QuantityMeasurementControllerTest
    {
        private Controllers.QuantityMeasurementController _controller;
        private QuantityMeasurementCacheRepository        _repository;

        [TestInitialize]
        public void SetUp()
        {
            _repository = QuantityMeasurementCacheRepository.GetInstance();
            _repository.DeleteAll();
            var service = new QuantityMeasurementServiceImpl(_repository);
            _controller = new Controllers.QuantityMeasurementController(service);
        }

        [TestCleanup]
        public void TearDown() => _repository.DeleteAll();

        [TestMethod]
        public void TestController_PerformComparison_DoesNotThrow()
        {
            // Controller catches all exceptions internally — should never throw
            _controller.PerformComparison(
                new QuantityDTO(1, "FEET",   "Length"),
                new QuantityDTO(12, "INCHES", "Length"));
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestController_PerformConversion_DoesNotThrow()
        {
            _controller.PerformConversion(new QuantityDTO(0, "CELSIUS", "Temperature"), "FAHRENHEIT");
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestController_PerformAddition_DoesNotThrow()
        {
            _controller.PerformAddition(
                new QuantityDTO(2, "FEET",   "Length"),
                new QuantityDTO(24, "INCHES", "Length"),
                "FEET");
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestController_PerformSubtraction_DoesNotThrow()
        {
            _controller.PerformSubtraction(
                new QuantityDTO(2, "FEET",   "Length"),
                new QuantityDTO(12, "INCHES", "Length"),
                "FEET");
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestController_PerformDivision_DoesNotThrow()
        {
            _controller.PerformDivision(
                new QuantityDTO(4, "FEET", "Length"),
                new QuantityDTO(2, "FEET", "Length"));
            Assert.AreEqual(1, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestController_NullService_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new Controllers.QuantityMeasurementController(null));
        }

        [TestMethod]
        public void TestController_TemperatureAdd_HandlesGracefully()
        {
            // Should NOT throw — controller swallows errors and prints them
            _controller.PerformAddition(
                new QuantityDTO(0, "CELSIUS",    "Temperature"),
                new QuantityDTO(32, "FAHRENHEIT", "Temperature"),
                "CELSIUS");
        }
    }

    // ── 9. DatabaseConfig ─────────────────────────────────────────────

    [TestClass]
    public class DatabaseConfigTest
    {
        [TestMethod]
        public void TestConfig_Singleton_ReturnsSameInstance()
        {
            var c1 = DatabaseConfig.GetInstance();
            var c2 = DatabaseConfig.GetInstance();
            Assert.AreSame(c1, c2);
        }

        [TestMethod]
        public void TestConfig_HasConnectionString()
        {
            string cs = DatabaseConfig.GetInstance().ConnectionString;
            Assert.IsNotNull(cs);
            Assert.IsTrue(cs.Length > 0);
        }

        [TestMethod]
        public void TestConfig_RepositoryTypeDefault()
        {
            string repoType = DatabaseConfig.GetInstance().RepositoryType;
            Assert.IsNotNull(repoType);
            Assert.IsTrue(
                repoType.Equals("database", StringComparison.OrdinalIgnoreCase) ||
                repoType.Equals("cache",    StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void TestConfig_MaxPoolSize_PositiveInt()
        {
            Assert.IsTrue(DatabaseConfig.GetInstance().MaxPoolSize > 0);
        }

        [TestMethod]
        public void TestConfig_GetProperty_WithDefault()
        {
            string val = DatabaseConfig.GetInstance().GetProperty("nonexistent.key", "DEFAULT");
            Assert.AreEqual("DEFAULT", val);
        }

        [TestMethod]
        public void TestConfig_GetIntProperty_WithDefault()
        {
            int val = DatabaseConfig.GetInstance().GetIntProperty("nonexistent.key", 42);
            Assert.AreEqual(42, val);
        }
    }

    // ── 10. SQL Injection Prevention ─────────────────────────────────

    [TestClass]
    public class SqlInjectionPreventionTest
    {
        private QuantityMeasurementCacheRepository _repository;
        private QuantityMeasurementServiceImpl     _service;

        [TestInitialize]
        public void SetUp()
        {
            _repository = QuantityMeasurementCacheRepository.GetInstance();
            _repository.DeleteAll();
            _service = new QuantityMeasurementServiceImpl(_repository);
        }

        [TestCleanup]
        public void TearDown() => _repository.DeleteAll();

        [TestMethod]
        public void TestSqlInjection_MaliciousUnitString_TreatedAsLiteral()
        {
            // The service should throw a QuantityMeasurementException for
            // an unknown unit name — NOT execute it as SQL
            var dto = new QuantityDTO(1, "'; DROP TABLE quantity_measurement_entity; --", "Length");
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Convert(dto, "INCHES"));
        }

        [TestMethod]
        public void TestSqlInjection_MaliciousCategory_TreatedAsLiteral()
        {
            var dto = new QuantityDTO(1, "FEET", "'; DROP TABLE users; --");
            Assert.ThrowsException<QuantityMeasurementException>(
                () => _service.Convert(dto, "INCHES"));
        }
    }

    // ── 11. Repository Type Switching ─────────────────────────────────

    [TestClass]
    public class RepositoryTypeSwitchingTest
    {
        [TestMethod]
        public void TestSwitch_CacheRepository_WorksCorrectly()
        {
            var repo    = QuantityMeasurementCacheRepository.GetInstance();
            repo.DeleteAll();
            var service = new QuantityMeasurementServiceImpl(repo);

            service.Convert(new QuantityDTO(1, "FEET", "Length"), "INCHES");
            Assert.AreEqual(1, repo.GetTotalCount());
            repo.DeleteAll();
        }

        [TestMethod]
        public void TestSwitch_ServiceIsRepositoryAgnostic()
        {
            var cacheRepo    = QuantityMeasurementCacheRepository.GetInstance();
            var cacheService = new QuantityMeasurementServiceImpl(cacheRepo);

            Assert.IsInstanceOfType(cacheRepo,
                typeof(IQuantityMeasurementRepository));
        }
    }

    // ── 12. Concurrency / thread-safety ───────────────────────────────

    [TestClass]
    public class ConcurrencyTest
    {
        [TestMethod]
        public void TestConcurrentAccess_CacheRepository_NoCorruption()
        {
            var repo = QuantityMeasurementCacheRepository.GetInstance();
            repo.DeleteAll();
            var service = new QuantityMeasurementServiceImpl(repo);

            const int threadCount = 10;
            var tasks = new Task[threadCount];
            var errors = new List<Exception>();
            var errLock = new object();

            for (int i = 0; i < threadCount; i++)
            {
                int idx = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        service.Convert(new QuantityDTO(idx, "FEET", "Length"), "INCHES");
                    }
                    catch (Exception ex)
                    {
                        lock (errLock) errors.Add(ex);
                    }
                });
            }

            Task.WaitAll(tasks);
            Assert.AreEqual(0, errors.Count, $"Errors during concurrent access: {string.Join(", ", errors)}");
            Assert.AreEqual(threadCount, repo.GetTotalCount());
            repo.DeleteAll();
        }
    }

    // ── 13. Large dataset performance ────────────────────────────────

    [TestClass]
    public class PerformanceTest
    {
        [TestMethod]
        [Timeout(10000)] // 10 seconds max
        public void TestLargeDataSet_100Entities_WithinTimeout()
        {
            var repo = QuantityMeasurementCacheRepository.GetInstance();
            repo.DeleteAll();
            var service = new QuantityMeasurementServiceImpl(repo);

            for (int i = 1; i <= 100; i++)
                service.Convert(new QuantityDTO(i, "FEET", "Length"), "INCHES");

            Assert.AreEqual(100, repo.GetTotalCount());
            var all = repo.GetAllMeasurements();
            Assert.AreEqual(100, all.Count);
            repo.DeleteAll();
        }
    }

    // ── 14. Integration test (cache-based, no DB required) ───────────

    [TestClass]
    public class QuantityMeasurementIntegrationTest
    {
        private QuantityMeasurementApp _app;
        private IQuantityMeasurementRepository _repository;

        [ClassInitialize]
        public static void SetUpEnvironment(TestContext ctx)
        {
            // Force cache mode so integration tests work without SQL Server
            Environment.SetEnvironmentVariable("APP_ENV", "testing");
        }

        [TestInitialize]
        public void SetUp()
        {
            _app        = QuantityMeasurementApp.GetInstance();
            _repository = _app.GetRepository();
            _repository.DeleteAll();
        }

        [TestCleanup]
        public void TearDown() => _repository.DeleteAll();

        [TestMethod]
        public void TestIntegration_EndToEnd_LengthComparison()
        {
            var controller = _app.GetController();
            var q1 = new QuantityDTO(1.0,  "FEET",   "Length");
            var q2 = new QuantityDTO(12.0, "INCHES", "Length");

            controller.PerformComparison(q1, q2);

            Assert.IsTrue(_repository.GetTotalCount() > 0);
            var results = _repository.GetMeasurementsByOperation("COMPARE");
            Assert.IsTrue(results.Count > 0);
        }

        [TestMethod]
        public void TestIntegration_EndToEnd_TemperatureConversion()
        {
            var controller = _app.GetController();
            var thisDto    = new QuantityDTO(0.0, "CELSIUS",    "Temperature");
            var thatDto    = new QuantityDTO(0.0, "FAHRENHEIT", "Temperature");

            controller.PerformConversion(thisDto, "FAHRENHEIT");

            var results = _repository.GetMeasurementsByOperation("CONVERT");
            Assert.IsTrue(results.Count > 0);
        }

        [TestMethod]
        public void TestIntegration_RepositoryPersistence_MultipleOps()
        {
            var controller = _app.GetController();

            controller.PerformAddition(
                new QuantityDTO(5.0,  "FEET",   "Length"),
                new QuantityDTO(10.0, "FEET",   "Length"),
                "FEET");
            controller.PerformConversion(
                new QuantityDTO(1.0,  "KILOGRAM", "Weight"),
                "GRAM");
            controller.PerformDivision(
                new QuantityDTO(10.0, "LITRE",  "Volume"),
                new QuantityDTO(2.0,  "LITRE",  "Volume"));

            Assert.IsTrue(_repository.GetTotalCount() >= 3);
        }

        [TestMethod]
        public void TestIntegration_GetMeasurementsByType()
        {
            var controller = _app.GetController();
            controller.PerformConversion(new QuantityDTO(1, "FEET",     "Length"),  "INCHES");
            controller.PerformConversion(new QuantityDTO(1, "KILOGRAM", "Weight"),  "GRAM");
            controller.PerformConversion(new QuantityDTO(1, "LITRE",    "Volume"),  "MILLILITRE");

            var lengthResults = _repository.GetMeasurementsByType("Length");
            Assert.IsTrue(lengthResults.Count >= 1);
        }

        [TestMethod]
        public void TestIntegration_DeleteAll_ClearsRepository()
        {
            var controller = _app.GetController();
            controller.PerformConversion(new QuantityDTO(1, "FEET", "Length"), "INCHES");
            Assert.IsTrue(_repository.GetTotalCount() > 0);

            _app.DeleteAllMeasurements();
            Assert.AreEqual(0, _repository.GetTotalCount());
        }

        [TestMethod]
        public void TestIntegration_AppSingleton_ReturnsSameInstance()
        {
            var app1 = QuantityMeasurementApp.GetInstance();
            var app2 = QuantityMeasurementApp.GetInstance();
            Assert.AreSame(app1, app2);
        }
    }

    // ── 15. Backward compatibility — UC15 test cases ─────────────────

    [TestClass]
    public class UC15BackwardCompatibilityTest
    {
        private QuantityMeasurementServiceImpl _service;
        private QuantityMeasurementCacheRepository _repository;

        [TestInitialize]
        public void SetUp()
        {
            _repository = QuantityMeasurementCacheRepository.GetInstance();
            _repository.DeleteAll();
            _service = new QuantityMeasurementServiceImpl(_repository);
        }

        [TestCleanup]
        public void TearDown() => _repository.DeleteAll();

        [TestMethod] public void UC15_1Foot_Equals_12Inches()      => Assert.IsTrue(_service.Compare(new QuantityDTO(1, "FEET", "Length"), new QuantityDTO(12, "INCHES", "Length")).ComparisonResult);
        [TestMethod] public void UC15_1Yard_Equals_3Feet()         => Assert.IsTrue(_service.Compare(new QuantityDTO(1, "YARDS", "Length"), new QuantityDTO(3, "FEET", "Length")).ComparisonResult);
        [TestMethod] public void UC15_1Kg_Equals_1000Gram()        => Assert.IsTrue(_service.Compare(new QuantityDTO(1, "KILOGRAM", "Weight"), new QuantityDTO(1000, "GRAM", "Weight")).ComparisonResult);
        [TestMethod] public void UC15_1Litre_Equals_1000Ml()       => Assert.IsTrue(_service.Compare(new QuantityDTO(1, "LITRE", "Volume"), new QuantityDTO(1000, "MILLILITRE", "Volume")).ComparisonResult);
        [TestMethod] public void UC15_0C_Equals_32F()              => Assert.IsTrue(_service.Compare(new QuantityDTO(0, "CELSIUS", "Temperature"), new QuantityDTO(32, "FAHRENHEIT", "Temperature")).ComparisonResult);
        [TestMethod] public void UC15_Add_2Feet_24Inches_Is_4Feet() => Assert.AreEqual(4.0, _service.Add(new QuantityDTO(2, "FEET", "Length"), new QuantityDTO(24, "INCHES", "Length"), "FEET").ResultValue, 0.001);
        [TestMethod] public void UC15_Divide_4Feet_By_2Feet_Is_2() => Assert.AreEqual(2.0, _service.Divide(new QuantityDTO(4, "FEET", "Length"), new QuantityDTO(2, "FEET", "Length")).ResultValue, 0.001);
        [TestMethod] public void UC15_Convert_1Foot_To_12Inches()   => Assert.AreEqual(12.0, _service.Convert(new QuantityDTO(1, "FEET", "Length"), "INCHES").ResultValue, 0.001);
        [TestMethod] public void UC15_Convert_100C_To_212F()        => Assert.AreEqual(212.0, _service.Convert(new QuantityDTO(100, "CELSIUS", "Temperature"), "FAHRENHEIT").ResultValue, 0.001);
        [TestMethod] public void UC15_Temperature_Add_Throws()      => Assert.ThrowsException<QuantityMeasurementException>(() => _service.Add(new QuantityDTO(0, "CELSIUS", "Temperature"), new QuantityDTO(32, "FAHRENHEIT", "Temperature"), "CELSIUS"));
    }
}
