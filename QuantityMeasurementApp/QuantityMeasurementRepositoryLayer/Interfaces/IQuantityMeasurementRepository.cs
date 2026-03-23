using System.Collections.Generic;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementRepositoryLayer.Interfaces
{
    public interface IQuantityMeasurementRepository
    {
        QuantityMeasurementEntity Save(QuantityMeasurementEntity entity);
        QuantityMeasurementEntity FindById(int index);
        List<QuantityMeasurementEntity> FindAll();
        bool Delete(int index);
    }
}
