namespace QuantityMeasurementApp.test;

[TestClass]
public sealed class FeetTest
{
    [TestMethod]
        public void TestEquality_SameValue()
        {
            // Arrange
            Feet feet1 = new Feet(1.0);
            Feet feet2 = new Feet(1.0);

            // Act
            bool result = feet1.Equals(feet2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestEquality_DifferentValue()
        {
            Feet feet1 = new Feet(1.0);
            Feet feet2 = new Feet(2.0);

            Assert.IsFalse(feet1.Equals(feet2));
        }

        [TestMethod]
        public void TestEquality_NullComparison()
        {
            Feet feet = new Feet(1.0);

            Assert.IsFalse(feet.Equals(null));
        }

        [TestMethod]
        public void TestEquality_SameReference()
        {
            Feet feet = new Feet(1.0);

            Assert.IsTrue(feet.Equals(feet));
        }

        [TestMethod]
        public void TestEquality_NonNumericInput()
        {
            Feet feet = new Feet(1.0);
            object nonNumeric = "Test";

            Assert.IsFalse(feet.Equals(nonNumeric));
        }
}
