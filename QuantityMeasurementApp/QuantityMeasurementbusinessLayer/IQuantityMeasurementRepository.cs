using System.Collections.Generic;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementbusinessLayer
{
    /// <summary>
    /// Repository contract lives in the business layer so both the service
    /// (business layer) and the concrete repositories (repository layer) can
    /// reference it without a circular dependency.
    ///
    /// Dependency direction:
    ///   ModelLayer  ←  BusinessLayer  ←  RepositoryLayer  ←  AppLayer
    /// </summary>
    public interface IQuantityMeasurementRepository
    {
        // ── CRUD ──────────────────────────────────────────────────────
        void Save(QuantityMeasurementEntity entity);
        QuantityMeasurementEntity? FindById(long id);
        List<QuantityMeasurementEntity> FindAll();
        bool Delete(long id);

        // ── UC16 additions ────────────────────────────────────────────
        List<QuantityMeasurementEntity> GetAllMeasurements();
        List<QuantityMeasurementEntity> GetMeasurementsByOperation(string operation);
        List<QuantityMeasurementEntity> GetMeasurementsByType(string measurementType);
        int  GetTotalCount();
        void DeleteAll();

        // ── Default methods ───────────────────────────────────────────
        string GetPoolStatistics() => "Pool statistics not available for this repository type.";
        void   ReleaseResources()  { }
    }
}
