using System;

namespace QuantityMeasurementApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter first value in feet:");
            double value1 = Convert.ToDouble(Console.ReadLine());

            Console.WriteLine("Enter second value in feet:");
            double value2 = Convert.ToDouble(Console.ReadLine());

            Feet feet1 = new Feet(value1);
            Feet feet2 = new Feet(value2);

            bool result = feet1.Equals(feet2);

            Console.WriteLine("Equal: " + result);
        }
    }
}