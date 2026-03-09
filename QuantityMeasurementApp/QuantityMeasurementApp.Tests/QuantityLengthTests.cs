using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityLengthTests
    {
        private const double EPS = 1e-6;

        [TestMethod]
        public void TestEquality_FeetAndInches()
        {
            var a = new QuantityLength(1, LengthUnit.FEET);
            var b = new QuantityLength(12, LengthUnit.INCHES);

            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void TestConvertTo_Inches()
        {
            var length = new QuantityLength(1, LengthUnit.FEET);
            var result = length.ConvertTo(LengthUnit.INCHES);

            Assert.AreEqual(12, result.Value, EPS);
        }

        [TestMethod]
        public void TestAddition_SameUnit()
        {
            var a = new QuantityLength(1, LengthUnit.FEET);
            var b = new QuantityLength(2, LengthUnit.FEET);

            var result = a.Add(b);

            Assert.AreEqual(3, result.Value, EPS);
        }

        [TestMethod]
        public void TestAddition_CrossUnit()
        {
            var a = new QuantityLength(1, LengthUnit.FEET);
            var b = new QuantityLength(12, LengthUnit.INCHES);

            var result = a.Add(b);

            Assert.AreEqual(2, result.Value, EPS);
        }

        [TestMethod]
        public void TestAddition_TargetUnit_Yards()
        {
            var a = new QuantityLength(1, LengthUnit.FEET);
            var b = new QuantityLength(12, LengthUnit.INCHES);

            var result = a.Add(b, LengthUnit.YARDS);

            Assert.AreEqual(0.666666, result.Value, 1e-5);
        }

        [TestMethod]
        public void TestConvertTo_Centimeters()
        {
            var a = new QuantityLength(1, LengthUnit.INCHES);
            var result = a.ConvertTo(LengthUnit.CENTIMETERS);

            Assert.AreEqual(2.54, result.Value, 1e-2);
        }
    }
}