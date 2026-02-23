using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;
using System;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class QuantityLengthTests
    {
        [TestMethod]
        public void TestEquality_FeetToFeet_SameValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Feet);
            var q2 = new QuantityLength(1.0, LengthUnit.Feet);

            Assert.IsTrue(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_InchToInch_SameValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Inch);
            var q2 = new QuantityLength(1.0, LengthUnit.Inch);

            Assert.IsTrue(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_FeetToInch_EquivalentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Feet);
            var q2 = new QuantityLength(12.0, LengthUnit.Inch);

            Assert.IsTrue(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_InchToFeet_EquivalentValue()
        {
            var q1 = new QuantityLength(12.0, LengthUnit.Inch);
            var q2 = new QuantityLength(1.0, LengthUnit.Feet);

            Assert.IsTrue(q1.Equals(q2));
        }

        [TestMethod]
        public void TestEquality_FeetToFeet_DifferentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Feet);
            var q2 = new QuantityLength(2.0, LengthUnit.Feet);

            Assert.IsFalse(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_InchToInch_DifferentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Inch);
            var q2 = new QuantityLength(2.0, LengthUnit.Inch);

            Assert.IsFalse(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_SameReference()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Feet);

            Assert.IsTrue(q1.Equals(q1));
        }

        [TestMethod]
        public void TestEquality_NullComparison()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Feet);

            Assert.IsFalse(q1.Equals(null));
        }

        [TestMethod]
        public void Test_InvalidUnit()
        {
            try
            {
                var q = new QuantityLength(1.0, (LengthUnit)999);
                Assert.Fail("Expected ArgumentException not thrown");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestEquality_YardToFeet_EquivalentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Yard);
            var q2 = new QuantityLength(3.0, LengthUnit.Feet);

            Assert.IsTrue(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_YardToInches_EquivalentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Yard);
            var q2 = new QuantityLength(36.0, LengthUnit.Inch);

            Assert.IsTrue(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_CentimeterToInch_EquivalentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Centimeter);
            var q2 = new QuantityLength(0.393701, LengthUnit.Inch);

            Assert.IsTrue(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_CentimeterToFeet_NonEquivalentValue()
        {
            var q1 = new QuantityLength(1.0, LengthUnit.Centimeter);
            var q2 = new QuantityLength(1.0, LengthUnit.Feet);

            Assert.IsFalse(q1 == q2);
        }

        [TestMethod]
        public void TestEquality_MultiUnit_TransitiveProperty()
        {
            var yard = new QuantityLength(1.0, LengthUnit.Yard);
            var feet = new QuantityLength(3.0, LengthUnit.Feet);
            var inches = new QuantityLength(36.0, LengthUnit.Inch);

            Assert.IsTrue(yard == feet);
            Assert.IsTrue(feet == inches);
            Assert.IsTrue(yard == inches);
        }
    }
}