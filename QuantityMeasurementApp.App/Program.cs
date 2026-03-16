using System;
using QuantityMeasurementApp.Core.Models;
using QuantityMeasurementApp.Core.Units;

namespace QuantityMeasurementApp.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var v1 = new Quantity<VolumeUnit>(1.0, VolumeUnit.LITRE);
            var v2 = new Quantity<VolumeUnit>(1000.0, VolumeUnit.MILLILITRE);
            var v3 = new Quantity<VolumeUnit>(1.0, VolumeUnit.GALLON);

            Console.WriteLine("1L == 1000mL : " + v1.Equals(v2));

            var convert = v1.ConvertTo(VolumeUnit.MILLILITRE);
            Console.WriteLine("1L -> mL = " + convert);

            var sum = v1.Add(v2, VolumeUnit.LITRE);
            Console.WriteLine("1L + 1000mL = " + sum);
        }
    }
}