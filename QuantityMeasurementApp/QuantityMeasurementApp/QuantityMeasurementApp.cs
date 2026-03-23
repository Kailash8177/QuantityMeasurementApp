using System;
using System.Collections.Generic;
using QuantityMeasurementApp.Controllers;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementApp.Menu;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Repositories;
using QuantityMeasurementRepositoryLayer.Util;

namespace QuantityMeasurementApp
{
    public sealed class QuantityMeasurementApp
    {
        private static QuantityMeasurementApp? _instance;
        private static readonly object _lock = new object();

        private readonly IQuantityMeasurementRepository _repository;
        private readonly IQuantityMeasurementService    _service;
        private readonly IQuantityMeasurementController _controller;

        private QuantityMeasurementApp()
        {
            var config      = DatabaseConfig.GetInstance();
            string repoType = config.RepositoryType;

            Console.WriteLine($"[App] Repository type configured: {repoType}");

            if (string.Equals(repoType, "database", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _repository = QuantityMeasurementDatabaseRepository.GetInstance();
                    Console.WriteLine("[App] Connected to SQL Server. Using Database Repository.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[App] SQL Server unavailable: {ex.Message}");
                    Console.WriteLine("[App] Switched to Cache Repository. Data will be saved to quantity_operations.json.");
                    _repository = QuantityMeasurementCacheRepository.GetInstance();
                }
            }
            else
            {
                _repository = QuantityMeasurementCacheRepository.GetInstance();
                Console.WriteLine("[App] Using Cache Repository. Data will be saved to quantity_operations.json.");
            }

            _service    = new QuantityMeasurementServiceImpl(_repository);
            _controller = new QuantityMeasurementController(_service);
            Console.WriteLine("[App] Application initialised successfully.");
        }

        public static QuantityMeasurementApp GetInstance()
        {
            if (_instance == null)
                lock (_lock)
                    if (_instance == null)
                        _instance = new QuantityMeasurementApp();
            return _instance;
        }

        public IQuantityMeasurementController GetController() => _controller;
        public IQuantityMeasurementRepository GetRepository() => _repository;

        public void Run()
        {
            var menu = new QuantityMeasurementMenu(_controller, _repository);
            menu.Show();
        }

        public void CloseResources()
        {
            _repository.ReleaseResources();
            Console.WriteLine("[App] Resources released.");
        }

        public void DeleteAllMeasurements() => _repository.DeleteAll();

        public void ReportAllMeasurements()
        {
            var all = _repository.GetAllMeasurements();
            Console.WriteLine($"\n[App] Total records: {all.Count}");
            foreach (var e in all)
                Console.WriteLine("  " + e);
            Console.WriteLine($"  {_repository.GetPoolStatistics()}");
        }

        public static void RunDemo()
        {
            var app        = GetInstance();
            var controller = app.GetController();

            controller.PerformComparison(
                new QuantityDTO(1, "FEET", "Length"),
                new QuantityDTO(12, "INCHES", "Length"));

            controller.PerformConversion(
                new QuantityDTO(0, "CELSIUS", "Temperature"), "FAHRENHEIT");

            controller.PerformAddition(
                new QuantityDTO(2, "FEET", "Length"),
                new QuantityDTO(24, "INCHES", "Length"), "FEET");

            controller.PerformDivision(
                new QuantityDTO(4, "FEET", "Length"),
                new QuantityDTO(2, "FEET", "Length"));

            app.ReportAllMeasurements();
            app.CloseResources();
        }
    }
}