using System;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementRepositoryLayer.Repositories;

namespace QuantityMeasurementApp.Menu
{
    public class QuantityMeasurementMenu
    {
        private readonly IQuantityMeasurementController   _controller;
        private readonly IQuantityMeasurementRepository   _repository;

        private static readonly string[] CATEGORIES   = { "Length", "Weight", "Volume", "Temperature" };
        private static readonly string[] LENGTH_UNITS = { "FEET", "INCHES", "YARDS", "CENTIMETERS" };
        private static readonly string[] WEIGHT_UNITS = { "KILOGRAM", "GRAM" };
        private static readonly string[] VOLUME_UNITS = { "LITRE", "MILLILITRE", "GALLON" };
        private static readonly string[] TEMP_UNITS   = { "CELSIUS", "FAHRENHEIT", "KELVIN" };

        public QuantityMeasurementMenu(
            IQuantityMeasurementController controller,
            IQuantityMeasurementRepository repository)
        {
            _controller = controller
                ?? throw new ArgumentNullException(nameof(controller));
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Show()
        {
            PrintWelcome();
            bool running = true;

            while (running)
            {
                PrintMainMenu();
                string? choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": HandleComparison();  break;
                    case "2": HandleConversion();  break;
                    case "3": HandleAddition();    break;
                    case "4": HandleSubtraction(); break;
                    case "5": HandleDivision();    break;
                    case "6": HandleStatus();      break;
                    case "7": running = false;     break;
                    default:  PrintError("Invalid option. Enter 1-7."); break;
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

        // ── Status ────────────────────────────────────────────────────

        private void HandleStatus()
        {
            Console.WriteLine("  +-------------------------------------------+");
            Console.WriteLine("  | APPLICATION STATUS                        |");
            Console.WriteLine("  +-------------------------------------------+");

            // Which repository is active
            string repoName = _repository.GetType().Name;
            bool isDatabase = _repository is QuantityMeasurementDatabaseRepository;
            bool isCache    = _repository is QuantityMeasurementCacheRepository;

            Console.WriteLine();
            if (isDatabase)
            {
                Console.WriteLine("  Storage mode  : SQL SERVER DATABASE");
                Console.WriteLine("  Status        : Online");
            }
            else if (isCache)
            {
                Console.WriteLine("  Storage mode  : CACHE  (saved to quantity_operations.json)");
                Console.WriteLine("  Status        : SQL Server offline — using local file fallback");
            }
            else
            {
                Console.WriteLine($"  Storage mode  : {repoName}");
            }

            // Record count
            try
            {
                int count = _repository.GetTotalCount();
                Console.WriteLine($"  Total records : {count}");
            }
            catch
            {
                Console.WriteLine("  Total records : unavailable");
            }

            // Pool / file stats
            Console.WriteLine($"  Details       : {_repository.GetPoolStatistics()}");
            Console.WriteLine();
        }

        // ── Operation handlers ────────────────────────────────────────

        private void HandleComparison()
        {
            PrintSectionHeader("COMPARE TWO QUANTITIES");
            var dto1 = ReadQuantityDTO("First quantity");
            if (dto1 == null) return;
            var dto2 = ReadQuantityDTOSameCategory("Second quantity", dto1.Category!);
            if (dto2 == null) return;
            Console.WriteLine(); PrintWorking();
            _controller.PerformComparison(dto1, dto2);
        }

        private void HandleConversion()
        {
            PrintSectionHeader("CONVERT A QUANTITY");
            var dto = ReadQuantityDTO("Source quantity");
            if (dto == null) return;
            string? target = ReadUnit("  Convert to unit", dto.Category!);
            if (target == null) return;
            Console.WriteLine(); PrintWorking();
            _controller.PerformConversion(dto, target);
        }

        private void HandleAddition()
        {
            PrintSectionHeader("ADD TWO QUANTITIES");
            var dto1 = ReadQuantityDTO("First quantity");
            if (dto1 == null) return;
            var dto2 = ReadQuantityDTOSameCategory("Second quantity", dto1.Category!);
            if (dto2 == null) return;
            string? target = ReadUnit("  Result unit", dto1.Category!);
            if (target == null) return;
            Console.WriteLine(); PrintWorking();
            _controller.PerformAddition(dto1, dto2, target);
        }

        private void HandleSubtraction()
        {
            PrintSectionHeader("SUBTRACT TWO QUANTITIES");
            var dto1 = ReadQuantityDTO("First quantity (minuend)");
            if (dto1 == null) return;
            var dto2 = ReadQuantityDTOSameCategory("Second quantity (subtrahend)", dto1.Category!);
            if (dto2 == null) return;
            string? target = ReadUnit("  Result unit", dto1.Category!);
            if (target == null) return;
            Console.WriteLine(); PrintWorking();
            _controller.PerformSubtraction(dto1, dto2, target);
        }

        private void HandleDivision()
        {
            PrintSectionHeader("DIVIDE TWO QUANTITIES");
            var dto1 = ReadQuantityDTO("Dividend");
            if (dto1 == null) return;
            var dto2 = ReadQuantityDTOSameCategory("Divisor", dto1.Category!);
            if (dto2 == null) return;
            Console.WriteLine(); PrintWorking();
            _controller.PerformDivision(dto1, dto2);
        }

        // ── Input helpers ─────────────────────────────────────────────

        private static QuantityDTO? ReadQuantityDTO(string label)
        {
            Console.WriteLine($"  {label}:");
            string? category = ReadCategory();
            if (category == null) return null;
            double value     = ReadValue();
            string? unit     = ReadUnit("  Unit", category);
            if (unit == null) return null;
            return new QuantityDTO(value, unit, category);
        }

        private static QuantityDTO? ReadQuantityDTOSameCategory(string label, string category)
        {
            Console.WriteLine($"\n  {label} [Category locked to: {category}]:");
            double value  = ReadValue();
            string? unit  = ReadUnit("  Unit", category);
            if (unit == null) return null;
            return new QuantityDTO(value, unit, category);
        }

        private static string? ReadCategory()
        {
            Console.WriteLine("\n  Select category:");
            Console.WriteLine("    1. Length\n    2. Weight\n    3. Volume\n    4. Temperature");
            Console.Write("  Enter number (1-4): ");
            string? input = Console.ReadLine();
            if (input == "1") return CATEGORIES[0];
            if (input == "2") return CATEGORIES[1];
            if (input == "3") return CATEGORIES[2];
            if (input == "4") return CATEGORIES[3];
            PrintError("Invalid category.");
            return null;
        }

        private static double ReadValue()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.Write("  Value: ");
                if (double.TryParse(Console.ReadLine(), out double v)) return v;
                PrintError("Enter a valid number.");
            }
            return 0;
        }

        private static string? ReadUnit(string prompt, string category)
        {
            string[] units = GetUnitsForCategory(category);
            Console.Write($"\n{prompt} for {category} — options: ");
            for (int i = 0; i < units.Length; i++)
            {
                if (i > 0) Console.Write(" | ");
                Console.Write($"{i + 1}. {units[i]}");
            }
            Console.WriteLine();
            Console.Write($"  Enter number (1-{units.Length}): ");
            if (int.TryParse(Console.ReadLine(), out int c) && c >= 1 && c <= units.Length)
                return units[c - 1];
            PrintError("Invalid unit selection.");
            return null;
        }

        private static string[] GetUnitsForCategory(string category) =>
            category.ToLower() switch
            {
                "length"      => LENGTH_UNITS,
                "weight"      => WEIGHT_UNITS,
                "volume"      => VOLUME_UNITS,
                "temperature" => TEMP_UNITS,
                _ => Array.Empty<string>()
            };

        // ── Print helpers ─────────────────────────────────────────────

        private static void PrintWelcome()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  |   QUANTITY MEASUREMENT APP  -  UC16    |");
            Console.WriteLine("  |  N-Tier + SQL Server Persistence        |");
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
            Console.WriteLine("  |  6.  Status    - Check storage mode     |");
            Console.WriteLine("  |  7.  Exit                               |");
            Console.WriteLine("  +-----------------------------------------+");
            Console.Write("  Choose (1-7): ");
        }

        private static void PrintSectionHeader(string title)
        {
            Console.WriteLine($"\n  +-------------------------------------------+");
            Console.WriteLine($"  | {title,-41} |");
            Console.WriteLine($"  +-------------------------------------------+");
        }

        private static void PrintWorking()   => Console.WriteLine("  Processing...\n");
        private static void PrintError(string msg) => Console.WriteLine($"  [!] {msg}");

        private static void PrintGoodbye()
        {
            Console.WriteLine();
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine("  |   Thank you for using Qty Measure UC16  |");
            Console.WriteLine("  +-----------------------------------------+");
            Console.WriteLine();
        }
    }
}