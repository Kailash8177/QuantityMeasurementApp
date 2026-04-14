using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;

namespace QuantityMeasurementRepositoryLayer.Repositories.EfCore
{
    public class EfQuantityMeasurementRepository : IQuantityMeasurementRepository
    {
        private readonly QuantityMeasurementDbContext _db;

        public EfQuantityMeasurementRepository(QuantityMeasurementDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public void Save(QuantityMeasurementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var record = ToRecord(entity);
            _db.Measurements.Add(record);
            _db.SaveChanges();

            entity.Id = record.Id;
        }

        public QuantityMeasurementEntity? FindById(long id)
        {
            var r = _db.Measurements.AsNoTracking().FirstOrDefault(x => x.Id == id);
            return r == null ? null : FromRecord(r);
        }

        public List<QuantityMeasurementEntity> FindAll() =>
            _db.Measurements
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(FromRecord)
                .ToList();

        public bool Delete(long id)
        {
            var r = _db.Measurements.FirstOrDefault(x => x.Id == id);
            if (r == null) return false;
            _db.Measurements.Remove(r);
            _db.SaveChanges();
            return true;
        }

        public List<QuantityMeasurementEntity> GetAllMeasurements() => FindAll();

        public List<QuantityMeasurementEntity> GetMeasurementsByOperation(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation)) return new List<QuantityMeasurementEntity>();
            var op = operation.Trim().ToUpperInvariant();

            return _db.Measurements
                .AsNoTracking()
                .Where(x => x.OperationType.ToUpper() == op)
                .OrderByDescending(x => x.CreatedAt)
                .Select(FromRecord)
                .ToList();
        }

        public List<QuantityMeasurementEntity> GetMeasurementsByType(string measurementType)
        {
            if (string.IsNullOrWhiteSpace(measurementType)) return new List<QuantityMeasurementEntity>();
            var type = measurementType.Trim();

            return _db.Measurements
                .AsNoTracking()
                .Where(x => x.Operand1Category == type)
                .OrderByDescending(x => x.CreatedAt)
                .Select(FromRecord)
                .ToList();
        }

        public int GetTotalCount() => _db.Measurements.Count();

        public void DeleteAll()
        {
            _db.Measurements.ExecuteDelete();
        }

        private static QuantityMeasurementRecord ToRecord(QuantityMeasurementEntity e)
        {
            if (e.Operand1 == null)
                throw new ArgumentException("Operand1 is required for persistence.");

            return new QuantityMeasurementRecord
            {
                Operand1Value = e.Operand1.Value,
                Operand1Unit = e.Operand1.Unit ?? "",
                Operand1Category = e.Operand1.Category ?? "",
                Operand2Value = e.Operand2?.Value,
                Operand2Unit = e.Operand2?.Unit,
                Operand2Category = e.Operand2?.Category,
                OperationType = e.OperationType ?? "UNKNOWN",
                ResultValue = e.IsComparison ? null : e.ResultValue,
                ResultUnit = e.ResultUnit,
                IsComparison = e.IsComparison,
                ComparisonResult = e.ComparisonResult,
                HasError = e.HasError,
                ErrorMessage = e.ErrorMessage,
                CreatedAt = e.CreatedAt == default ? DateTime.UtcNow : e.CreatedAt
            };
        }

        private static QuantityMeasurementEntity FromRecord(QuantityMeasurementRecord r)
        {
            var op1 = new QuantityDTO(r.Operand1Value, r.Operand1Unit, r.Operand1Category);
            QuantityDTO? op2 = null;
            if (r.Operand2Value.HasValue && r.Operand2Unit != null && r.Operand2Category != null)
                op2 = new QuantityDTO(r.Operand2Value.Value, r.Operand2Unit, r.Operand2Category);

            return new QuantityMeasurementEntity(
                r.Id,
                op1,
                op2,
                r.OperationType,
                r.ResultValue ?? 0,
                r.ResultUnit,
                r.IsComparison,
                r.ComparisonResult,
                r.HasError,
                r.ErrorMessage,
                r.CreatedAt);
        }
    }
}

