using System;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementModelLayer.DTOs;

namespace QuantityMeasurementApp.Menu
{
    public class QuantityMeasurementMenu
    {
        private IQuantityMeasurementController _controller;

        private static readonly string[] CATEGORIES   = { "Length", "Weight", "Volume", "Temperature" };
        private static readonly string[] LENGTH_UNITS = { "FEET", "INCHES", "YARDS", "CENTIMETERS" };
        private static readonly string[] WEIGHT_UNITS = { "KILOGRAM", "GRAM" };
        private static readonly string[] VOLUME_UNITS = { "LITRE", "MILLILITRE", "GALLON" };
        private static readonly string[] TEMP_UNITS   = { "CELSIUS", "FAHRENHEIT", "KELVIN" };

        public QuantityMeasurementMenu(IQuantityMeasurementController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller cannot be null");
            }
            _controller = controller;
        }

        // ============================================================
        // Main menu loop
        // ============================================================

        public void Show()
        {
            PrintWelcome();
            bool running = true;

            while (running)
            {
                PrintMainMenu();
                string choice = Console.ReadLine();
                Console.WriteLine();

                if (choice == "1")
                {
                    HandleComparison();
                }
                else if (choice == "2")
                {
                    HandleConversion();
                }
                else if (choice == "3")
                {
                    HandleAddition();
                }
                else if (choice == "4")
                {
                    HandleSubtraction();
                }
                else if (choice == "5")
                {
                    HandleDivision();
                }
                else if (choice == "6")
                {
                    running = false;
                }
                else
                {
                    PrintError("Invalid option. Please enter a number from 1 to 6.");
                }

                if (running)
                {
                    Console.WriteLine();
                    Console.Write("  Press ENTER to continue...");
                    Console.ReadLine();
                }
            }

            PrintGoodbye();
        }

        // ============================================================
        // Operation handlers
        // ============================================================

        private void HandleComparison()
        {
            PrintSectionHeader("COMPARE TWO QUANTITIES");
            Console.WriteLine("  Checks whether two quantities are equal after unit conversion.");
            Console.WriteLine();

            QuantityDTO dto1 = ReadQuantityDTO("First quantity");
            if (dto1 == null) { return; }

            QuantityDTO dto2 = ReadQuantityDTOSameCategory("Second quantity", dto1.Category);
            if (dto2 == null) { return; }

            Console.WriteLine();
            PrintWorking();
            _controller.PerformComparison(dto1, dto2);
        }

        private void HandleConversion()
        {
            PrintSectionHeader("CONVERT A QUANTITY");
            Console.WriteLine("  Converts a value from one unit to another within the same category.");
            Console.WriteLine();

            QuantityDTO dto = ReadQuantityDTO("Source quantity");
            if (dto == null) { return; }

            Console.WriteLine();
            string targetUnit = ReadUnit("  Convert to unit", dto.Category);
            if (targetUnit == null) { return; }

            Console.WriteLine();
            PrintWorking();
            _controller.PerformConversion(dto, targetUnit);
        }

        private void HandleAddition()
        {
            PrintSectionHeader("ADD TWO QUANTITIES");
            Console.WriteLine("  Adds two quantities and returns the result in your chosen unit.");
            Console.WriteLine();

            QuantityDTO dto1 = ReadQuantityDTO("First quantity");
            if (dto1 == null) { return; }

            QuantityDTO dto2 = ReadQuantityDTOSameCategory("Second quantity", dto1.Category);
            if (dto2 == null) { return; }

            Console.WriteLine();
            string targetUnit = ReadUnit("  Result unit", dto1.Category);
            if (targetUnit == null) { return; }

            Console.WriteLine();
            PrintWorking();
            _controller.PerformAddition(dto1, dto2, targetUnit);
        }

        private void HandleSubtraction()
        {
            PrintSectionHeader("SUBTRACT TWO QUANTITIES");
            Console.WriteLine("  Subtracts the second quantity from the first.");
            Console.WriteLine();

            QuantityDTO dto1 = ReadQuantityDTO("First quantity (minuend)");
            if (dto1 == null) { return; }

            QuantityDTO dto2 = ReadQuantityDTOSameCategory("Second quantity (subtrahend)", dto1.Category);
            if (dto2 == null) { return; }

            Console.WriteLine();
            string targetUnit = ReadUnit("  Result unit", dto1.Category);
            if (targetUnit == null) { return; }

            Console.WriteLine();
            PrintWorking();
            _controller.PerformSubtraction(dto1, dto2, targetUnit);
        }

        private void HandleDivision()
        {
            PrintSectionHeader("DIVIDE TWO QUANTITIES");
            Console.WriteLine("  Divides the first quantity by the second. Returns a dimensionless ratio.");
            Console.WriteLine();

            QuantityDTO dto1 = ReadQuantityDTO("Dividend (quantity to divide)");
            if (dto1 == null) { return; }

            QuantityDTO dto2 = ReadQuantityDTOSameCategory("Divisor (quantity to divide by)", dto1.Category);
            if (dto2 == null) { return; }

            Console.WriteLine();
            PrintWorking();
            _controller.PerformDivision(dto1, dto2);
        }

        // ============================================================
        // Guided input helpers
        // ============================================================

        private static QuantityDTO ReadQuantityDTO(string label)
        {
            Console.WriteLine("  " + label + ":");
            Console.WriteLine("  " + new string('-', label.Length + 1));

            string category = ReadCategory();
            if (category == null)
            {
                return null;
            }

            double value = ReadValue();

            string unit = ReadUnit("  Unit", category);
            if (unit == null)
            {
                return null;
            }

            return new QuantityDTO(value, unit, category);
        }

        private static QuantityDTO ReadQuantityDTOSameCategory(string label, string category)
        {
            Console.WriteLine();
            Console.WriteLine("  " + label + " [Category locked to: " + category + "]");
            Console.WriteLine("  " + new string('-', label.Length + 1));

            double value = ReadValue();

            string unit = ReadUnit("  Unit", category);
            if (unit == null)
            {
                return null;
            }

            return new QuantityDTO(value, unit, category);
        }

        private static string ReadCategory()
        {
            Console.WriteLine();
            Console.WriteLine("  Select category:");
            Console.WriteLine("    1. Length");
            Console.WriteLine("    2. Weight");
            Console.WriteLine("    3. Volume");
            Console.WriteLine("    4. Temperature");
            Console.Write("  Enter number (1-4): ");

            string input = Console.ReadLine();

            if (input == "1") { return CATEGORIES[0]; }
            if (input == "2") { return CATEGORIES[1]; }
            if (input == "3") { return CATEGORIES[2]; }
            if (input == "4") { return CATEGORIES[3]; }

            PrintError("Invalid category selection.");
            return null;
        }

        private static double ReadValue()
        {
            double value = 0;
            bool valid   = false;
            int attempts = 0;

            while (!valid && attempts < 3)
            {
                Console.Write("  Value: ");
                string input = Console.ReadLine();

                if (double.TryParse(input, out value))
                {
                    valid = true;
                }
                else
                {
                    attempts++;
                    PrintError("Please enter a valid number (e.g. 100 or 3.14).");
                }
            }

            return value;
        }

        private static string ReadUnit(string prompt, string category)
        {
            string[] units = GetUnitsForCategory(category);

            Console.WriteLine();
            Console.Write(prompt + " for " + category + " — options: ");

            for (int i = 0; i < units.Length; i++)
            {
                if (i > 0) { Console.Write(" | "); }
                Console.Write((i + 1) + ". " + units[i]);
            }

            Console.WriteLine();
            Console.Write("  Enter number (1-" + units.Length + "): ");

            string input = Console.ReadLine();
            int choice   = 0;

            if (int.TryParse(input, out choice) && choice >= 1 && choice <= units.Length)
            {
                return units[choice - 1];
            }

            PrintError("Invalid unit selection.");
            return null;
        }

        private static string[] GetUnitsForCategory(string category)
        {
            if (category == null)
            {
                return new string[0];
            }

            string lower = category.ToLower();

            if (lower == "length")
            {
                return LENGTH_UNITS;
            }
            if (lower == "weight")
            {
                return WEIGHT_UNITS;
            }
            if (lower == "volume")
            {
                return VOLUME_UNITS;
            }
            if (lower == "temperature")
            {
                return TEMP_UNITS;
            }

            return new string[0];
        }

        // ============================================================
        // Print helpers
        // ============================================================

        private static void PrintWelcome()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  |    QUANTITY MEASUREMENT APP  -  UC15    |");
            Console.WriteLine("  |     N-Tier Architecture Edition         |");
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  Supports: Length | Weight | Volume | Temperature");
            Console.WriteLine();
        }

        private static void PrintMainMenu()
        {
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  |              MAIN MENU                  |");
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  |  1.  Compare   - Are two values equal?  |");
            Console.WriteLine("  |  2.  Convert   - Change unit            |");
            Console.WriteLine("  |  3.  Add       - Sum two quantities     |");
            Console.WriteLine("  |  4.  Subtract  - Difference             |");
            Console.WriteLine("  |  5.  Divide    - Ratio of two values    |");
            Console.WriteLine("  |  6.  Exit                               |");
            Console.WriteLine("  +-----------------------------------------+");
            Console.Write("  Choose (1-6): ");
        }

        private static void PrintSectionHeader(string title)
        {
            int pad     = (41 - title.Length) / 2;
            string line = new string('-', 43);
            Console.WriteLine("  +" + line + "+");
            Console.WriteLine("  |" + new string(' ', pad) + title + new string(' ', 43 - pad - title.Length) + "|");
            Console.WriteLine("  +" + line + "+");
        }

        private static void PrintWorking()
        {
            Console.WriteLine("  Processing...");
            Console.WriteLine();
        }

        private static void PrintError(string message)
        {
            Console.WriteLine("  [!] " + message);
        }

        private static void PrintGoodbye()
        {
            Console.WriteLine();
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  |   Thank you for using Qty Measure UC15  |");
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine();
        }
    }
}