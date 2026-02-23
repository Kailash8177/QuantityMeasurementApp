using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantityMeasurementApp;

namespace QuantityMeasurementApp.Tests
{
    [TestClass]
    public class InchesTests
    {
        [TestMethod]
        public void TestEquality_SameValue()
        {
            Assert.IsTrue(new Inches(1.0).Equals(new Inches(1.0)));
        }

        [TestMethod]
        public void TestEquality_DifferentValue()
        {
            Assert.IsFalse(new Inches(1.0).Equals(new Inches(2.0)));
        }

        [TestMethod]
        public void TestEquality_NullComparison()
        {
            Assert.IsFalse(new Inches(1.0).Equals(null));
        }

        [TestMethod]
        public void TestEquality_SameReference()
        {
            Inches inch = new Inches(1.0);
            Assert.IsTrue(inch.Equals(inch));
        }

        [TestMethod]
        public void TestEquality_NonNumericInput()
        {
            Assert.IsFalse(new Inches(1.0).Equals("Test"));
        }
    }
}