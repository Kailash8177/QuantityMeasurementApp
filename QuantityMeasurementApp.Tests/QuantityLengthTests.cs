using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;
using System;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityLengthAdditionTests
    {
        private const double EPSILON = 1e-6;

        // ===============================
        // SAME UNIT ADDITION
        // ===============================

        [TestMethod]
        public void TestAddition_SameUnit_FeetPlusFeet()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(2.0, LengthUnit.Feet);

            var result = a.Add(b);

            Assert.AreEqual(3.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Feet, result.Unit);
        }

        [TestMethod]
        public void TestAddition_SameUnit_InchPlusInch()
        {
            var a = new QuantityLength(6.0, LengthUnit.Inch);
            var b = new QuantityLength(6.0, LengthUnit.Inch);

            var result = a.Add(b);

            Assert.AreEqual(12.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Inch, result.Unit);
        }

        // ===============================
        // CROSS UNIT ADDITION
        // ===============================

        [TestMethod]
        public void TestAddition_CrossUnit_FeetPlusInches()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            var result = a.Add(b);

            Assert.AreEqual(2.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Feet, result.Unit);
        }

        [TestMethod]
        public void TestAddition_CrossUnit_InchesPlusFeet()
        {
            var a = new QuantityLength(12.0, LengthUnit.Inch);
            var b = new QuantityLength(1.0, LengthUnit.Feet);

            var result = a.Add(b);

            Assert.AreEqual(24.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Inch, result.Unit);
        }

        [TestMethod]
        public void TestAddition_CrossUnit_YardPlusFeet()
        {
            var a = new QuantityLength(1.0, LengthUnit.Yard);
            var b = new QuantityLength(3.0, LengthUnit.Feet);

            var result = a.Add(b);

            Assert.AreEqual(2.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Yard, result.Unit);
        }

        [TestMethod]
        public void TestAddition_CrossUnit_CentimeterPlusInch()
        {
            var a = new QuantityLength(2.54, LengthUnit.Centimeter);
            var b = new QuantityLength(1.0, LengthUnit.Inch);

            var result = a.Add(b);

            Assert.AreEqual(5.08, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Centimeter, result.Unit);
        }

        // ===============================
        // COMMUTATIVITY
        // ===============================

        [TestMethod]
        public void TestAddition_Commutativity()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);
            var b = new QuantityLength(12.0, LengthUnit.Inch);

            var result1 = a.Add(b);
            var result2 = b.Add(a).ConvertTo(LengthUnit.Feet);

            Assert.IsTrue(result1 == result2);
        }

        // ===============================
        // ZERO IDENTITY
        // ===============================

        [TestMethod]
        public void TestAddition_WithZero()
        {
            var a = new QuantityLength(5.0, LengthUnit.Feet);
            var zero = new QuantityLength(0.0, LengthUnit.Inch);

            var result = a.Add(zero);

            Assert.AreEqual(5.0, result.Value, EPSILON);
            Assert.AreEqual(LengthUnit.Feet, result.Unit);
        }

        // ===============================
        // NEGATIVE VALUES
        // ===============================

        [TestMethod]
        public void TestAddition_NegativeValues()
        {
            var a = new QuantityLength(5.0, LengthUnit.Feet);
            var b = new QuantityLength(-2.0, LengthUnit.Feet);

            var result = a.Add(b);

            Assert.AreEqual(3.0, result.Value, EPSILON);
        }

        // ===============================
        // LARGE VALUES
        // ===============================

        [TestMethod]
        public void TestAddition_LargeValues()
        {
            var a = new QuantityLength(1e6, LengthUnit.Feet);
            var b = new QuantityLength(1e6, LengthUnit.Feet);

            var result = a.Add(b);

            Assert.AreEqual(2e6, result.Value, EPSILON);
        }

        // ===============================
        // SMALL VALUES
        // ===============================

        [TestMethod]
        public void TestAddition_SmallValues()
        {
            var a = new QuantityLength(0.001, LengthUnit.Feet);
            var b = new QuantityLength(0.002, LengthUnit.Feet);

            var result = a.Add(b);

            Assert.AreEqual(0.003, result.Value, EPSILON);
        }

        // ===============================
        // NULL HANDLING
        // ===============================

        [TestMethod]
        public void TestAddition_NullSecondOperand()
        {
            var a = new QuantityLength(1.0, LengthUnit.Feet);

            try
            {
                a.Add(null);
                Assert.Fail("Expected ArgumentException not thrown.");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }
    }
}