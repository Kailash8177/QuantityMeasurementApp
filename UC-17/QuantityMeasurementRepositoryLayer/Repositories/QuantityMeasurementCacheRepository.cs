using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;

namespace QuantityMeasurementRepositoryLayer.Repositories
{
    /// <summary>
    /// In-memory cache repository that automatically persists its contents
    /// to quantity_operations.json after every Save/Delete operation.
    /// On startup it reloads any previously saved records from that file,
    /// so data survives application restarts even without SQL Server.
    /// </summary>
    public sealed class QuantityMeasurementCacheRepository : IQuantityMeasurementRepository
    {
        // ── Singleton ─────────────────────────────────────────────────
        private static QuantityMeasurementCacheRepository? _instance;
        private static readonly object _lock = new object();

        private readonly object _sync = new object();
        private readonly List<QuantityMeasurementEntity> _cache = new List<QuantityMeasurementEntity>();
        private long _nextId = 1;

        private static readonly string FILE_PATH =
            Path.Combine(AppContext.BaseDirectory, "quantity_operations.json");

        // ── Constructor ───────────────────────────────────────────────

        private QuantityMeasurementCacheRepository()
        {
            LoadFromFile();
            Console.WriteLine($"[CacheRepository] Initialised. Loaded {_cache.Count} records from file.");
        }

        public static QuantityMeasurementCacheRepository GetInstance()
        {
            if (_instance == null)
                lock (_lock)
                    if (_instance == null)
                        _instance = new QuantityMeasurementCacheRepository();
            return _instance;
        }

        // ── Save ──────────────────────────────────────────────────────

        public void Save(QuantityMeasurementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            lock (_sync)
            {
                entity.Id = _nextId++;
                _cache.Add(entity);
                SaveToFile_NoLock();
                Console.WriteLine($"[CacheRepository] Saved entity id={entity.Id}");
            }
        }

        // ── FindById ──────────────────────────────────────────────────

        public QuantityMeasurementEntity? FindById(long id)
        {
            lock (_sync) return _cache.FirstOrDefault(e => e.Id == id);
        }

        // ── FindAll ───────────────────────────────────────────────────

        public List<QuantityMeasurementEntity> FindAll()
        {
            lock (_sync) return new List<QuantityMeasurementEntity>(_cache);
        }

        // ── Delete ────────────────────────────────────────────────────

        public bool Delete(long id)
        {
            lock (_sync)
            {
                var entity = _cache.FirstOrDefault(e => e.Id == id);
                if (entity == null) return false;
                _cache.Remove(entity);
                SaveToFile_NoLock();
                return true;
            }
        }

        // ── UC16 query methods ────────────────────────────────────────

        public List<QuantityMeasurementEntity> GetAllMeasurements()
        {
            lock (_sync) return new List<QuantityMeasurementEntity>(_cache);
        }

        public List<QuantityMeasurementEntity> GetMeasurementsByOperation(string operation)
        {
            lock (_sync)
                return _cache
                    .Where(e => string.Equals(e.OperationType, operation, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }

        public List<QuantityMeasurementEntity> GetMeasurementsByType(string measurementType)
        {
            lock (_sync)
                return _cache
                    .Where(e => e.Operand1 != null &&
                                string.Equals(e.Operand1.Category, measurementType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }

        public int GetTotalCount()
        {
            lock (_sync) return _cache.Count;
        }

        public void DeleteAll()
        {
            lock (_sync)
            {
                _cache.Clear();
                SaveToFile_NoLock();
                Console.WriteLine("[CacheRepository] All records deleted.");
            }
        }

        public string GetPoolStatistics() =>
            $"CacheRepository[count={_cache.Count}, file={FILE_PATH}]";

        public void ReleaseResources()
        {
            lock (_sync)
            {
                SaveToFile_NoLock();
                Console.WriteLine("[CacheRepository] Cache flushed to file.");
            }
        }

        // ── JSON persistence ──────────────────────────────────────────

        private void SaveToFile_NoLock()
        {
            try
            {
                var records = _cache.Select(ToRecord).ToList();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(records, options);
                File.WriteAllText(FILE_PATH, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CacheRepository] Warning: could not save to file: {ex.Message}");
            }
        }

        private void LoadFromFile()
        {
            try
            {
                if (!File.Exists(FILE_PATH)) return;

                string json = File.ReadAllText(FILE_PATH, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return;

                var records = JsonSerializer.Deserialize<List<JsonRecord>>(json);
                if (records == null) return;

                foreach (var r in records)
                {
                    var entity = FromRecord(r);
                    _cache.Add(entity);
                    if (r.Id >= _nextId)
                        _nextId = r.Id + 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CacheRepository] Warning: could not load from file: {ex.Message}");
                _cache.Clear();
                _nextId = 1;
            }
        }

        // ── Mapping ───────────────────────────────────────────────────

        private static JsonRecord ToRecord(QuantityMeasurementEntity e) => new JsonRecord
        {
            Id                  = e.Id,
            Operation           = e.OperationType,
            CreatedAt           = e.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            IsComparison        = e.IsComparison,
            ComparisonResult    = e.ComparisonResult,
            HasError            = e.HasError,
            ErrorMessage        = e.ErrorMessage,
            ResultValue         = e.ResultValue,
            ResultUnit          = e.ResultUnit,
            ThisValue           = e.Operand1?.Value ?? 0,
            ThisUnit            = e.Operand1?.Unit,
            ThisMeasurementType = e.Operand1?.Category,
            HasOperand2         = e.Operand2 != null,
            ThatValue           = e.Operand2?.Value ?? 0,
            ThatUnit            = e.Operand2?.Unit,
            ThatMeasurementType = e.Operand2?.Category
        };

        private static QuantityMeasurementEntity FromRecord(JsonRecord r)
        {
            QuantityDTO? op1 = r.ThisUnit != null
                ? new QuantityDTO(r.ThisValue, r.ThisUnit, r.ThisMeasurementType ?? "")
                : null;

            QuantityDTO? op2 = r.HasOperand2 && r.ThatUnit != null
                ? new QuantityDTO(r.ThatValue, r.ThatUnit, r.ThatMeasurementType ?? "")
                : null;

            return new QuantityMeasurementEntity(
                r.Id, op1, op2, r.Operation ?? "UNKNOWN",
                r.ResultValue, r.ResultUnit,
                r.IsComparison, r.ComparisonResult,
                r.HasError, r.ErrorMessage,
                DateTime.TryParse(r.CreatedAt, out var dt) ? dt : DateTime.UtcNow);
        }

        // ── JSON record shape ─────────────────────────────────────────

        private class JsonRecord
        {
            public long    Id                  { get; set; }
            public string? Operation           { get; set; }
            public string? CreatedAt           { get; set; }
            public bool    IsComparison        { get; set; }
            public bool    ComparisonResult    { get; set; }
            public bool    HasError            { get; set; }
            public string? ErrorMessage        { get; set; }
            public double  ResultValue         { get; set; }
            public string? ResultUnit          { get; set; }
            public double  ThisValue           { get; set; }
            public string? ThisUnit            { get; set; }
            public string? ThisMeasurementType { get; set; }
            public bool    HasOperand2         { get; set; }
            public double  ThatValue           { get; set; }
            public string? ThatUnit            { get; set; }
            public string? ThatMeasurementType { get; set; }
        }
    }
}

