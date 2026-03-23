using QuantityMeasurementApp.Controllers;
using QuantityMeasurementApp.Interface;
using QuantityMeasurementApp.Menu;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Repositories;

namespace QuantityMeasurementApp
{
    public class QuantityMeasurementApp
    {
        private static QuantityMeasurementApp _instance;
        private static readonly object _lock = new object();

        private IQuantityMeasurementController _controller;

        private QuantityMeasurementApp()
        {
            _controller = CreateController();
        }

        public static QuantityMeasurementApp GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new QuantityMeasurementApp();
                    }
                }
            }
            return _instance;
        }

        private static IQuantityMeasurementRepository CreateRepository()
        {
            return QuantityMeasurementCacheRepository.GetInstance();
        }

        private static IQuantityMeasurementService CreateService()
        {
            return new QuantityMeasurementServiceImpl(CreateRepository());
        }

        private static IQuantityMeasurementController CreateController()
        {
            return new QuantityMeasurementController(CreateService());
        }

        public IQuantityMeasurementController GetController()
        {
            return _controller;
        }

        public void Run()
        {
            QuantityMeasurementMenu menu = new QuantityMeasurementMenu(_controller);
            menu.Show();
        }
    }
}
