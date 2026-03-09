using System;

namespace QuantityMeasurementApp
{
    class QuantityMeasurementApp
    {
        static void Main(string[] args)
        {
            var length1 = new QuantityLength(1, LengthUnit.FEET);
            var length2 = new QuantityLength(12, LengthUnit.INCHES);

            Console.WriteLine(length1.Equals(length2));

            var weight1 = new QuantityWeight(1, WeightUnit.KILOGRAM);
            var weight2 = new QuantityWeight(1000, WeightUnit.GRAM);

            Console.WriteLine(weight1.Equals(weight2));
        }
    }
}