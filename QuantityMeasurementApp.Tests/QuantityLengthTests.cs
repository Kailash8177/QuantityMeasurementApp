using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;
using System;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityLengthAdditionExplicitTargetTests
    {
        private const double EPSILON = 1e-6;

        [TestMethod]
        public void TestAddition_ExplicitTargetUnit_Feet()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            var result = a.Add(b, LengthUnit.Feet);

            Assert.AreEqual(2.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Feet, result.Unit);
        }

        [TestMethod]
        public void TestAddition_ExplicitTargetUnit_Inches()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            var result = a.Add(b, LengthUnit.Inch);

            Assert.AreEqual(24.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Inch, result.Unit);
        }

        [TestMethod]
        public void TestAddition_ExplicitTargetUnit_Yards()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            var result = a.Add(b, LengthUnit.Yard);

            Assert.AreEqual(0.666666, result.Value, 1e-5);
            Assert.AreEqual(LengthUnit.Yard, result.Unit);
        }

        [TestMethod]
        public void TestAddition_ExplicitTargetUnit_Centimeters()
        {
            var a = new QuantityLength(1.0, LengthUnit.Inch);
            var b = new QuantityLength(1.0, LengthUnit.Inch);

            var result = a.Add(b, LengthUnit.Centimeter);

            Assert.AreEqual(5.08, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Centimeter, result.Unit);
        }

        [TestMethod]
        public void TestAddition_Commutativity_WithTargetUnit()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            var r1 = a.Add(b, LengthUnit.Yard);
            var r2 = b.Add(a, LengthUnit.Yard);

            Assert.IsTrue(r1 == r2);
        }

        [TestMethod]
        public void TestAddition_WithZero_TargetUnit()
        {
            var a = new QuantityLength(5.0, LengthUnit.Feet);
            var zero = new QuantityLength(0.0, LengthUnit.Inch);

            var result = a.Add(zero, LengthUnit.Yard);

            Assert.AreEqual(1.666666, result.Value, 1e-5);
        }

        [TestMethod]
        public void TestAddition_NegativeValues_TargetUnit()
        {
            var a = new QuantityLength(5.0, LengthUnit.Feet);
            var b = new QuantityLength(-2.0, LengthUnit.Feet);

            var result = a.Add(b, LengthUnit.Inch);

            Assert.AreEqual(36.0, result.Value, EPSILON);
        }

        [TestMethod]
        public void TestAddition_NullTargetUnit()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            try
            {
                a.Add(b, (LengthUnit)999);
                Assert.Fail("Expected exception not thrown.");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }
    }
}