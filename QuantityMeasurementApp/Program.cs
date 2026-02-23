using System;

namespace QuantityMeasurementApp
{
    class Program
    {
        public static bool CompareFeet(double value1, double value2)
        {
            Feet feet1 = new Feet(value1);
            Feet feet2 = new Feet(value2);
            return feet1.Equals(feet2);
        }

        public static bool CompareInches(double value1, double value2)
        {
            Inches inch1 = new Inches(value1);
            Inches inch2 = new Inches(value2);
            return inch1.Equals(inch2);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Feet Equal: " + CompareFeet(1.0, 1.0));
            Console.WriteLine("Inches Equal: " + CompareInches(1.0, 1.0));
        }
    }
}