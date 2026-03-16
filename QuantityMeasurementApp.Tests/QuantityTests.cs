using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp.Core.Models;
using QuantityMeasurementApp.Core.Units;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityTests
    {
        [TestMethod]
        public void Test_LengthEquality()
        {
            var a = new Quantity<LengthUnit>(1, LengthUnit.FEET);
            var b = new Quantity<LengthUnit>(12, LengthUnit.INCHES);

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void Test_WeightEquality()
        {
            var a = new Quantity<WeightUnit>(1, WeightUnit.KILOGRAM);
            var b = new Quantity<WeightUnit>(1000, WeightUnit.GRAM);

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void Test_LengthConversion()
        {
            var a = new Quantity<LengthUnit>(1, LengthUnit.FEET);

            var result = a.ConvertTo(LengthUnit.INCHES);

            Assert.AreEqual(12, result.Value);
        }

        [TestMethod]
        public void Test_WeightConversion()
        {
            var a = new Quantity<WeightUnit>(1, WeightUnit.KILOGRAM);

            var result = a.ConvertTo(WeightUnit.GRAM);

            Assert.AreEqual(1000, result.Value);
        }

        [TestMethod]
        public void Test_Addition_Length()
        {
            var a = new Quantity<LengthUnit>(1, LengthUnit.FEET);
            var b = new Quantity<LengthUnit>(12, LengthUnit.INCHES);

            var result = a.Add(b, LengthUnit.FEET);

            Assert.AreEqual(2, result.Value);
        }

        [TestMethod]
        public void Test_Addition_Weight()
        {
            var a = new Quantity<WeightUnit>(1, WeightUnit.KILOGRAM);
            var b = new Quantity<WeightUnit>(1000, WeightUnit.GRAM);

            var result = a.Add(b, WeightUnit.KILOGRAM);

            Assert.AreEqual(2, result.Value);
        }
    }
}