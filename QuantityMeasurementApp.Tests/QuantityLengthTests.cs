using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;
using System;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityLengthConversionTests
    {
        private const double EPSILON = 1e-6;

        [TestMethod]
        public void TestConversion_FeetToInches()
        {
            double result = QuantityLength.Convert(1.0, LengthUnit.Feet, LengthUnit.Inch);
            Assert.AreEqual(12.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_InchesToFeet()
        {
            double result = QuantityLength.Convert(24.0, LengthUnit.Inch, LengthUnit.Feet);
            Assert.AreEqual(2.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_YardsToInches()
        {
            double result = QuantityLength.Convert(1.0, LengthUnit.Yard, LengthUnit.Inch);
            Assert.AreEqual(36.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_CentimeterToInches()
        {
            double result = QuantityLength.Convert(2.54, LengthUnit.Centimeter, LengthUnit.Inch);
            Assert.AreEqual(1.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_FeetToYard()
        {
            double result = QuantityLength.Convert(6.0, LengthUnit.Feet, LengthUnit.Yard);
            Assert.AreEqual(2.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_ZeroValue()
        {
            double result = QuantityLength.Convert(0.0, LengthUnit.Feet, LengthUnit.Inch);
            Assert.AreEqual(0.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_NegativeValue()
        {
            double result = QuantityLength.Convert(-1.0, LengthUnit.Feet, LengthUnit.Inch);
            Assert.AreEqual(-12.0, result, EPSILON);
        }

        [TestMethod]
        public void TestConversion_RoundTrip_PreservesValue()
        {
            double original = 5.0;

            double inches = QuantityLength.Convert(original, LengthUnit.Feet, LengthUnit.Inch);
            double backToFeet = QuantityLength.Convert(inches, LengthUnit.Inch, LengthUnit.Feet);

            Assert.AreEqual(original, backToFeet, EPSILON);
        }

        [TestMethod]
        public void TestConversion_InvalidValue_Throws()
        {
            try
            {
                QuantityLength.Convert(double.NaN, LengthUnit.Feet, LengthUnit.Inch);
                Assert.Fail("Expected ArgumentException not thrown.");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }
    }
}