using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityWeightTests
    {
        [TestMethod]
        public void Test_WeightEquality()
        {
            var a = new QuantityWeight(1, WeightUnit.KILOGRAM);
            var b = new QuantityWeight(1000, WeightUnit.GRAM);

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void Test_WeightConversion()
        {
            var w = new QuantityWeight(1, WeightUnit.KILOGRAM);
            var result = w.ConvertTo(WeightUnit.GRAM);

            Assert.AreEqual(1000, result.Value, 0.001);
        }
    }
}