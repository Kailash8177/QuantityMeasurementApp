using QuantityMeasurementApp.Core.Models;
using QuantityMeasurementApp.Core.Units;

namespace QuantityMeasurementApp.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Length Operations ===");

            var l1 = new Quantity<LengthUnit>(1, LengthUnit.FEET);
            var l2 = new Quantity<LengthUnit>(12, LengthUnit.INCHES);

            Console.WriteLine(l1.Equals(l2));

            Console.WriteLine(l1.ConvertTo(LengthUnit.INCHES));

            Console.WriteLine(l1.Add(l2, LengthUnit.FEET));

            Console.WriteLine("\n=== Weight Operations ===");

            var w1 = new Quantity<WeightUnit>(1, WeightUnit.KILOGRAM);
            var w2 = new Quantity<WeightUnit>(1000, WeightUnit.GRAM);

            Console.WriteLine(w1.Equals(w2));

            Console.WriteLine(w1.ConvertTo(WeightUnit.GRAM));

            Console.WriteLine(w1.Add(w2, WeightUnit.KILOGRAM));
        }
    }
}