using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityLengthTests
    {
        [TestMethod]
        public void Test_LengthEquality()
        {
            var a = new QuantityLength(1, LengthUnit.FEET);
            var b = new QuantityLength(12, LengthUnit.INCHES);

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void Test_LengthConversion()
        {
            var a = new QuantityLength(1, LengthUnit.FEET);
            var result = a.ConvertTo(LengthUnit.INCHES);

            Assert.AreEqual(12, result.Value, 0.001);
        }
    }
}