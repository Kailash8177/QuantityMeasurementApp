using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;

namespace QuantityMeasurementRepositoryLayer.Repositories
{
    // ================================================================
    // Flat JSON record — used only for file serialization.
    // All fields are public with get/set so System.Text.Json can
    // read and write them without any special configuration.
    // ================================================================

    public class QuantityMeasurementJsonRecord
    {
        public int    Id               { get; set; }

        // Operand 1
        public double Operand1Value    { get; set; }
        public string Operand1Unit     { get; set; }
        public string Operand1Category { get; set; }

        // Operand 2 (null for single-operand operations)
        public bool   HasOperand2      { get; set; }
        public double Operand2Value    { get; set; }
        public string Operand2Unit     { get; set; }
        public string Operand2Category { get; set; }

        // Operation metadata
        public string OperationType    { get; set; }
        public bool   IsComparison     { get; set; }
        public bool   ComparisonResult { get; set; }

        // Result
        public double ResultValue      { get; set; }
        public string ResultUnit       { get; set; }

        // Error
        public bool   HasError         { get; set; }
        public string ErrorMessage     { get; set; }

        // Timestamp
        public string CreatedAt        { get; set; }
    }

    // ================================================================
    // Singleton cache repository with JSON file persistence.
    // On startup  : loads existing records from the JSON file.
    // On Save()   : appends the new record and rewrites the file.
    // On Delete() : removes the record and rewrites the file.
    // ================================================================

    public class QuantityMeasurementCacheRepository : IQuantityMeasurementRepository
    {
        // ── Singleton ─────────────────────────────────────────────────
        private static QuantityMeasurementCacheRepository _instance;
        private static readonly object _lock = new object();

        // ── Storage ───────────────────────────────────────────────────
        private List<QuantityMeasurementEntity> _cache;
        private List<QuantityMeasurementJsonRecord> _jsonRecords;
        private int _nextId;

        // JSON file path — written in the same folder as the executable
        private static readonly string JSON_FILE_PATH = "quantity_operations.json";

        // ── Private constructor ───────────────────────────────────────
        private QuantityMeasurementCacheRepository()
        {
            _cache       = new List<QuantityMeasurementEntity>();
            _jsonRecords = new List<QuantityMeasurementJsonRecord>();
            _nextId      = 1;
            LoadFromFile();
        }

        // ── Singleton factory ─────────────────────────────────────────
        public static QuantityMeasurementCacheRepository GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new QuantityMeasurementCacheRepository();
                    }
                }
            }
            return _instance;
        }

        // ============================================================
        // IQuantityMeasurementRepository
        // ============================================================

        public QuantityMeasurementEntity Save(QuantityMeasurementEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity cannot be null");
            }

            _cache.Add(entity);

            QuantityMeasurementJsonRecord record = ToJsonRecord(entity, _nextId);
            _nextId++;

            _jsonRecords.Add(record);
            SaveToFile();

            return entity;
        }

        public QuantityMeasurementEntity FindById(int index)
        {
            if (index < 0 || index >= _cache.Count)
            {
                return null;
            }
            return _cache[index];
        }

        public List<QuantityMeasurementEntity> FindAll()
        {
            List<QuantityMeasurementEntity> result = new List<QuantityMeasurementEntity>();
            for (int i = 0; i < _cache.Count; i++)
            {
                result.Add(_cache[i]);
            }
            return result;
        }

        public bool Delete(int index)
        {
            if (index < 0 || index >= _cache.Count)
            {
                return false;
            }
            _cache.RemoveAt(index);
            _jsonRecords.RemoveAt(index);
            SaveToFile();
            return true;
        }

        // ============================================================
        // JSON file — Save
        // Writes the full _jsonRecords list to the file as a JSON array.
        // Uses manual string building — no LINQ, no modern syntax.
        // ============================================================

        private void SaveToFile()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[");

                for (int i = 0; i < _jsonRecords.Count; i++)
                {
                    QuantityMeasurementJsonRecord r = _jsonRecords[i];

                    sb.AppendLine("  {");
                    sb.AppendLine("    \"Id\": "               + r.Id + ",");
                    sb.AppendLine("    \"OperationType\": \""  + Escape(r.OperationType) + "\",");
                    sb.AppendLine("    \"CreatedAt\": \""      + Escape(r.CreatedAt) + "\",");
                    sb.AppendLine("    \"IsComparison\": "     + BoolStr(r.IsComparison) + ",");
                    sb.AppendLine("    \"ComparisonResult\": " + BoolStr(r.ComparisonResult) + ",");
                    sb.AppendLine("    \"HasError\": "         + BoolStr(r.HasError) + ",");
                    sb.AppendLine("    \"ErrorMessage\": "     + NullableStr(r.ErrorMessage) + ",");
                    sb.AppendLine("    \"ResultValue\": "      + r.ResultValue + ",");
                    sb.AppendLine("    \"ResultUnit\": "       + NullableStr(r.ResultUnit) + ",");

                    // Operand 1
                    sb.AppendLine("    \"Operand1Value\": "    + r.Operand1Value + ",");
                    sb.AppendLine("    \"Operand1Unit\": "     + NullableStr(r.Operand1Unit) + ",");
                    sb.AppendLine("    \"Operand1Category\": " + NullableStr(r.Operand1Category) + ",");

                    // Operand 2
                    sb.AppendLine("    \"HasOperand2\": "      + BoolStr(r.HasOperand2) + ",");
                    sb.AppendLine("    \"Operand2Value\": "    + r.Operand2Value + ",");
                    sb.AppendLine("    \"Operand2Unit\": "     + NullableStr(r.Operand2Unit) + ",");
                    sb.AppendLine("    \"Operand2Category\": " + NullableStr(r.Operand2Category));

                    if (i < _jsonRecords.Count - 1)
                    {
                        sb.AppendLine("  },");
                    }
                    else
                    {
                        sb.AppendLine("  }");
                    }
                }

                sb.AppendLine("]");

                File.WriteAllText(JSON_FILE_PATH, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Repository] Warning: could not save to file. " + ex.Message);
            }
        }

        // ============================================================
        // JSON file — Load
        // Reads the JSON file on startup and rebuilds _cache and
        // _jsonRecords using a simple manual line-by-line parser.
        // ============================================================

        private void LoadFromFile()
        {
            try
            {
                if (!File.Exists(JSON_FILE_PATH))
                {
                    return;
                }

                string content = File.ReadAllText(JSON_FILE_PATH, Encoding.UTF8);

                if (content == null || content.Trim().Length == 0)
                {
                    return;
                }

                List<QuantityMeasurementJsonRecord> loaded = ParseJsonArray(content);

                for (int i = 0; i < loaded.Count; i++)
                {
                    QuantityMeasurementJsonRecord r = loaded[i];
                    _jsonRecords.Add(r);
                    _cache.Add(FromJsonRecord(r));

                    if (r.Id >= _nextId)
                    {
                        _nextId = r.Id + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Repository] Warning: could not load from file. " + ex.Message);
                _cache.Clear();
                _jsonRecords.Clear();
                _nextId = 1;
            }
        }

        // ============================================================
        // Manual JSON array parser
        // Splits the file content into individual object blocks then
        // reads each field line by line.
        // ============================================================

        private static List<QuantityMeasurementJsonRecord> ParseJsonArray(string json)
        {
            List<QuantityMeasurementJsonRecord> list = new List<QuantityMeasurementJsonRecord>();

            // Split into object blocks by finding matching { }
            int pos = 0;
            while (pos < json.Length)
            {
                int start = json.IndexOf('{', pos);
                if (start < 0) { break; }

                int depth = 0;
                int end   = start;

                for (int i = start; i < json.Length; i++)
                {
                    if (json[i] == '{') { depth++; }
                    if (json[i] == '}') { depth--; }
                    if (depth == 0) { end = i; break; }
                }

                string block = json.Substring(start, end - start + 1);
                QuantityMeasurementJsonRecord record = ParseJsonObject(block);
                list.Add(record);

                pos = end + 1;
            }

            return list;
        }

        private static QuantityMeasurementJsonRecord ParseJsonObject(string block)
        {
            QuantityMeasurementJsonRecord r = new QuantityMeasurementJsonRecord();

            r.Id               = ReadInt(block, "Id");
            r.OperationType    = ReadString(block, "OperationType");
            r.CreatedAt        = ReadString(block, "CreatedAt");
            r.IsComparison     = ReadBool(block, "IsComparison");
            r.ComparisonResult = ReadBool(block, "ComparisonResult");
            r.HasError         = ReadBool(block, "HasError");
            r.ErrorMessage     = ReadString(block, "ErrorMessage");
            r.ResultValue      = ReadDouble(block, "ResultValue");
            r.ResultUnit       = ReadString(block, "ResultUnit");
            r.Operand1Value    = ReadDouble(block, "Operand1Value");
            r.Operand1Unit     = ReadString(block, "Operand1Unit");
            r.Operand1Category = ReadString(block, "Operand1Category");
            r.HasOperand2      = ReadBool(block, "HasOperand2");
            r.Operand2Value    = ReadDouble(block, "Operand2Value");
            r.Operand2Unit     = ReadString(block, "Operand2Unit");
            r.Operand2Category = ReadString(block, "Operand2Category");

            return r;
        }

        // ── Field readers ─────────────────────────────────────────────

        private static string ReadString(string block, string key)
        {
            string search = "\"" + key + "\": ";
            int idx = block.IndexOf(search);
            if (idx < 0) { return null; }

            int valueStart = idx + search.Length;

            // Check if value is null
            if (valueStart < block.Length && block.Substring(valueStart, 4) == "null")
            {
                return null;
            }

            // Value is a quoted string
            int quoteOpen = block.IndexOf('"', valueStart);
            if (quoteOpen < 0) { return null; }

            int quoteClose = quoteOpen + 1;
            while (quoteClose < block.Length)
            {
                if (block[quoteClose] == '\\')
                {
                    quoteClose += 2;
                    continue;
                }
                if (block[quoteClose] == '"') { break; }
                quoteClose++;
            }

            return block.Substring(quoteOpen + 1, quoteClose - quoteOpen - 1)
                        .Replace("\\\"", "\"")
                        .Replace("\\\\", "\\");
        }

        private static int ReadInt(string block, string key)
        {
            string search = "\"" + key + "\": ";
            int idx = block.IndexOf(search);
            if (idx < 0) { return 0; }

            int valueStart = idx + search.Length;
            int valueEnd   = valueStart;

            while (valueEnd < block.Length
                   && block[valueEnd] != ','
                   && block[valueEnd] != '\n'
                   && block[valueEnd] != '}')
            {
                valueEnd++;
            }

            string raw = block.Substring(valueStart, valueEnd - valueStart).Trim();
            int result = 0;
            int.TryParse(raw, out result);
            return result;
        }

        private static double ReadDouble(string block, string key)
        {
            string search = "\"" + key + "\": ";
            int idx = block.IndexOf(search);
            if (idx < 0) { return 0; }

            int valueStart = idx + search.Length;
            int valueEnd   = valueStart;

            while (valueEnd < block.Length
                   && block[valueEnd] != ','
                   && block[valueEnd] != '\n'
                   && block[valueEnd] != '}')
            {
                valueEnd++;
            }

            string raw = block.Substring(valueStart, valueEnd - valueStart).Trim();
            double result = 0;
            double.TryParse(raw, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out result);
            return result;
        }

        private static bool ReadBool(string block, string key)
        {
            string search = "\"" + key + "\": ";
            int idx = block.IndexOf(search);
            if (idx < 0) { return false; }

            int valueStart = idx + search.Length;
            int valueEnd   = valueStart;

            while (valueEnd < block.Length
                   && block[valueEnd] != ','
                   && block[valueEnd] != '\n'
                   && block[valueEnd] != '}')
            {
                valueEnd++;
            }

            string raw = block.Substring(valueStart, valueEnd - valueStart).Trim();
            return raw == "true";
        }

        // ============================================================
        // Conversion helpers — Entity <-> JsonRecord
        // ============================================================

        private static QuantityMeasurementJsonRecord ToJsonRecord(
            QuantityMeasurementEntity entity, int id)
        {
            QuantityMeasurementJsonRecord r = new QuantityMeasurementJsonRecord();

            r.Id            = id;
            r.OperationType = entity.OperationType;
            r.CreatedAt     = entity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            r.IsComparison  = entity.IsComparison;
            r.HasError      = entity.HasError;
            r.ErrorMessage  = entity.ErrorMessage;

            if (entity.IsComparison)
            {
                r.ComparisonResult = entity.ComparisonResult;
            }
            else
            {
                r.ResultValue = entity.ResultValue;
                r.ResultUnit  = entity.ResultUnit;
            }

            if (entity.Operand1 != null)
            {
                r.Operand1Value    = entity.Operand1.Value;
                r.Operand1Unit     = entity.Operand1.Unit;
                r.Operand1Category = entity.Operand1.Category;
            }

            if (entity.Operand2 != null)
            {
                r.HasOperand2      = true;
                r.Operand2Value    = entity.Operand2.Value;
                r.Operand2Unit     = entity.Operand2.Unit;
                r.Operand2Category = entity.Operand2.Category;
            }

            return r;
        }

        private static QuantityMeasurementEntity FromJsonRecord(
            QuantityMeasurementJsonRecord r)
        {
            QuantityDTO op1 = null;
            QuantityDTO op2 = null;

            if (r.Operand1Unit != null)
            {
                op1 = new QuantityDTO(r.Operand1Value, r.Operand1Unit, r.Operand1Category);
            }

            if (r.HasOperand2 && r.Operand2Unit != null)
            {
                op2 = new QuantityDTO(r.Operand2Value, r.Operand2Unit, r.Operand2Category);
            }

            if (r.HasError)
            {
                return new QuantityMeasurementEntity(r.OperationType, r.ErrorMessage);
            }

            if (r.IsComparison)
            {
                return new QuantityMeasurementEntity(op1, op2, r.ComparisonResult);
            }

            if (op2 != null)
            {
                return new QuantityMeasurementEntity(
                    op1, op2, r.OperationType, r.ResultValue, r.ResultUnit);
            }

            return new QuantityMeasurementEntity(
                op1, r.OperationType, r.ResultValue, r.ResultUnit);
        }

        // ============================================================
        // JSON string helpers
        // ============================================================

        private static string Escape(string value)
        {
            if (value == null) { return ""; }
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string BoolStr(bool value)
        {
            return value ? "true" : "false";
        }

        private static string NullableStr(string value)
        {
            if (value == null) { return "null"; }
            return "\"" + Escape(value) + "\"";
        }
    }
}