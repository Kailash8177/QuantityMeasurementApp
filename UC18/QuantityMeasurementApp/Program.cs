using System;
using QuantityMeasurementApp;

namespace QuantityMeasurementApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--demo")
            {
                QuantityMeasurementApp.RunDemo();
                return;
            }

            var app = QuantityMeasurementApp.GetInstance();
            app.Run();

            // UC16: report all measurements stored before shutting down
            app.ReportAllMeasurements();
            app.CloseResources();
        }
    }
}