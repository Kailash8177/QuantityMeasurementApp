using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Util;

namespace QuantityMeasurementRepositoryLayer.Repositories
{
    public sealed class QuantityMeasurementDatabaseRepository : IQuantityMeasurementRepository
    {
        private static QuantityMeasurementDatabaseRepository? _instance;
        private static readonly object _instanceLock = new object();
        private readonly ConnectionPool _pool;

        private const string SP_SAVE        = "sp_SaveMeasurement";
        private const string SP_GET_ALL     = "sp_GetAllMeasurements";
        private const string SP_GET_BY_ID   = "sp_GetMeasurementById";
        private const string SP_GET_BY_OP   = "sp_GetMeasurementsByOperation";
        private const string SP_GET_BY_TYPE = "sp_GetMeasurementsByType";
        private const string SP_COUNT       = "sp_GetTotalCount";
        private const string SP_DELETE      = "sp_DeleteMeasurement";
        private const string SP_DELETE_ALL  = "sp_DeleteAllMeasurements";

        private QuantityMeasurementDatabaseRepository()
        {
            _pool = ConnectionPool.GetInstance();
            Console.WriteLine("[DatabaseRepository] Initialised.");
            try { InitialiseDatabase(); }
            catch (Exception ex)
            {
                throw new DatabaseException($"Database init failed: {ex.Message}", ex);
            }
        }

        public static QuantityMeasurementDatabaseRepository GetInstance()
        {
            if (_instance == null)
                lock (_instanceLock)
                    if (_instance == null)
                    {
                        try { _instance = new QuantityMeasurementDatabaseRepository(); }
                        catch { _instance = null; throw; }
                    }
            return _instance;
        }

        private void InitialiseDatabase()
        {
            const string ddl = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='quantity_measurement_entity')
                BEGIN
                    CREATE TABLE quantity_measurement_entity (
                        id                  BIGINT IDENTITY(1,1)  NOT NULL,
                        operand1_value      FLOAT                 NOT NULL,
                        operand1_unit       NVARCHAR(50)          NOT NULL,
                        operand1_category   NVARCHAR(50)          NOT NULL,
                        operand2_value      FLOAT                 NULL,
                        operand2_unit       NVARCHAR(50)          NULL,
                        operand2_category   NVARCHAR(50)          NULL,
                        operation_type      NVARCHAR(20)          NOT NULL,
                        result_value        FLOAT                 NULL,
                        result_unit         NVARCHAR(50)          NULL,
                        is_comparison       BIT                   NOT NULL DEFAULT 0,
                        comparison_result   BIT                   NOT NULL DEFAULT 0,
                        has_error           BIT                   NOT NULL DEFAULT 0,
                        error_message       NVARCHAR(500)         NULL,
                        created_at          DATETIME2             NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT PK_qme PRIMARY KEY (id)
                    );
                    CREATE INDEX idx_op_type ON quantity_measurement_entity(operation_type);
                    CREATE INDEX idx_op1_cat ON quantity_measurement_entity(operand1_category);
                    CREATE INDEX idx_created ON quantity_measurement_entity(created_at);
                END";

            var conn = _pool.GetConnection();
            try
            {
                using var cmd = new SqlCommand(ddl, conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine("[DatabaseRepository] Table verified.");
                EnsureStoredProcedures(conn);
            }
            catch (Exception ex) { Console.WriteLine($"[DatabaseRepository] Warning: {ex.Message}"); }
            finally { _pool.ReleaseConnection(conn); }
        }

        private static void EnsureStoredProcedures(SqlConnection conn)
        {
            var sps = new[]
            {
                (@"IF OBJECT_ID('sp_SaveMeasurement','P') IS NULL EXEC('
                CREATE PROCEDURE sp_SaveMeasurement
                  @operand1_value FLOAT,@operand1_unit NVARCHAR(50),@operand1_category NVARCHAR(50),
                  @operand2_value FLOAT=NULL,@operand2_unit NVARCHAR(50)=NULL,@operand2_category NVARCHAR(50)=NULL,
                  @operation_type NVARCHAR(20),
                  @result_value FLOAT=NULL,@result_unit NVARCHAR(50)=NULL,
                  @is_comparison BIT=0,@comparison_result BIT=0,
                  @has_error BIT=0,@error_message NVARCHAR(500)=NULL
                AS BEGIN SET NOCOUNT ON;
                  INSERT INTO quantity_measurement_entity
                    (operand1_value,operand1_unit,operand1_category,
                     operand2_value,operand2_unit,operand2_category,
                     operation_type,result_value,result_unit,
                     is_comparison,comparison_result,has_error,error_message,created_at)
                  VALUES
                    (@operand1_value,@operand1_unit,@operand1_category,
                     @operand2_value,@operand2_unit,@operand2_category,
                     @operation_type,@result_value,@result_unit,
                     @is_comparison,@comparison_result,@has_error,@error_message,GETUTCDATE());
                  SELECT SCOPE_IDENTITY() AS NewId;
                END')"),

                (@"IF OBJECT_ID('sp_GetAllMeasurements','P') IS NULL EXEC('
                CREATE PROCEDURE sp_GetAllMeasurements AS BEGIN SET NOCOUNT ON;
                  SELECT * FROM quantity_measurement_entity ORDER BY created_at DESC; END')"),

                (@"IF OBJECT_ID('sp_GetMeasurementById','P') IS NULL EXEC('
                CREATE PROCEDURE sp_GetMeasurementById @id BIGINT AS BEGIN SET NOCOUNT ON;
                  SELECT * FROM quantity_measurement_entity WHERE id=@id; END')"),

                (@"IF OBJECT_ID('sp_GetMeasurementsByOperation','P') IS NULL EXEC('
                CREATE PROCEDURE sp_GetMeasurementsByOperation @operation_type NVARCHAR(20)
                AS BEGIN SET NOCOUNT ON;
                  SELECT * FROM quantity_measurement_entity WHERE operation_type=@operation_type
                  ORDER BY created_at DESC; END')"),

                (@"IF OBJECT_ID('sp_GetMeasurementsByType','P') IS NULL EXEC('
                CREATE PROCEDURE sp_GetMeasurementsByType @category NVARCHAR(50)
                AS BEGIN SET NOCOUNT ON;
                  SELECT * FROM quantity_measurement_entity WHERE operand1_category=@category
                  ORDER BY created_at DESC; END')"),

                (@"IF OBJECT_ID('sp_GetTotalCount','P') IS NULL EXEC('
                CREATE PROCEDURE sp_GetTotalCount AS BEGIN SET NOCOUNT ON;
                  SELECT COUNT(*) AS TotalCount FROM quantity_measurement_entity; END')"),

                (@"IF OBJECT_ID('sp_DeleteMeasurement','P') IS NULL EXEC('
                CREATE PROCEDURE sp_DeleteMeasurement @id BIGINT AS BEGIN SET NOCOUNT ON;
                  DELETE FROM quantity_measurement_entity WHERE id=@id;
                  SELECT @@ROWCOUNT AS RowsAffected; END')"),

                (@"IF OBJECT_ID('sp_DeleteAllMeasurements','P') IS NULL EXEC('
                CREATE PROCEDURE sp_DeleteAllMeasurements AS BEGIN SET NOCOUNT ON;
                  DELETE FROM quantity_measurement_entity; END')")
            };

            foreach (var sp in sps)
            {
                using var cmd = new SqlCommand(sp, conn);
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("[DatabaseRepository] Stored procedures verified.");
        }

        // ── Save ──────────────────────────────────────────────────────

        public void Save(QuantityMeasurementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var conn = _pool.GetConnection();
            try
            {
                using var cmd = new SqlCommand(SP_SAVE, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@operand1_value",    entity.Operand1?.Value    ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operand1_unit",     entity.Operand1?.Unit     ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operand1_category", entity.Operand1?.Category ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operand2_value",    entity.Operand2 != null   ? (object)entity.Operand2.Value    : DBNull.Value);
                cmd.Parameters.AddWithValue("@operand2_unit",     entity.Operand2?.Unit     ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operand2_category", entity.Operand2?.Category ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@operation_type",    entity.OperationType      ?? "UNKNOWN");
                cmd.Parameters.AddWithValue("@result_value",      entity.IsComparison       ? (object)DBNull.Value : entity.ResultValue);
                cmd.Parameters.AddWithValue("@result_unit",       entity.ResultUnit         ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@is_comparison",     entity.IsComparison       ? 1 : 0);
                cmd.Parameters.AddWithValue("@comparison_result", entity.ComparisonResult   ? 1 : 0);
                cmd.Parameters.AddWithValue("@has_error",         entity.HasError           ? 1 : 0);
                cmd.Parameters.AddWithValue("@error_message",     entity.ErrorMessage       ?? (object)DBNull.Value);

                entity.Id = Convert.ToInt64(cmd.ExecuteScalar());
                Console.WriteLine($"[DatabaseRepository] Saved id={entity.Id}, op={entity.OperationType}");
            }
            catch (DatabaseException) { throw; }
            catch (Exception ex) { throw DatabaseException.QueryFailed(SP_SAVE, ex); }
            finally { _pool.ReleaseConnection(conn); }
        }

        // ── FindById ──────────────────────────────────────────────────

        public QuantityMeasurementEntity? FindById(long id)
        {
            var conn = _pool.GetConnection();
            SqlCommand? cmd = null; SqlDataReader? reader = null;
            try
            {
                cmd = new SqlCommand(SP_GET_BY_ID, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                reader = cmd.ExecuteReader();
                return reader.Read() ? MapRow(reader) : null;
            }
            catch (Exception ex) { throw DatabaseException.QueryFailed(SP_GET_BY_ID, ex); }
            finally { CloseResources(reader, cmd, conn); }
        }

        public List<QuantityMeasurementEntity> FindAll() =>
            ExecuteSpList(SP_GET_ALL, null);

        public bool Delete(long id)
        {
            var conn = _pool.GetConnection();
            try
            {
                using var cmd = new SqlCommand(SP_DELETE, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch (Exception ex) { throw DatabaseException.QueryFailed(SP_DELETE, ex); }
            finally { _pool.ReleaseConnection(conn); }
        }

        public List<QuantityMeasurementEntity> GetAllMeasurements()
        {
            var r = ExecuteSpList(SP_GET_ALL, null);
            Console.WriteLine($"[DatabaseRepository] Retrieved {r.Count} measurements.");
            return r;
        }

        public List<QuantityMeasurementEntity> GetMeasurementsByOperation(string operation) =>
            ExecuteSpList(SP_GET_BY_OP, cmd => cmd.Parameters.AddWithValue("@operation_type", operation));

        public List<QuantityMeasurementEntity> GetMeasurementsByType(string measurementType) =>
            ExecuteSpList(SP_GET_BY_TYPE, cmd => cmd.Parameters.AddWithValue("@category", measurementType));

        public int GetTotalCount()
        {
            var conn = _pool.GetConnection();
            try
            {
                using var cmd = new SqlCommand(SP_COUNT, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex) { throw DatabaseException.QueryFailed(SP_COUNT, ex); }
            finally { _pool.ReleaseConnection(conn); }
        }

        public void DeleteAll()
        {
            var conn = _pool.GetConnection();
            try
            {
                using var cmd = new SqlCommand(SP_DELETE_ALL, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                Console.WriteLine("[DatabaseRepository] All records deleted.");
            }
            catch (Exception ex) { throw DatabaseException.TransactionFailed(SP_DELETE_ALL, ex); }
            finally { _pool.ReleaseConnection(conn); }
        }

        public string GetPoolStatistics() => _pool.GetPoolStatistics();

        public void ReleaseResources()
        {
            _pool.CloseAll();
            Console.WriteLine("[DatabaseRepository] Connection pool closed.");
        }

        // ── Helpers ───────────────────────────────────────────────────

        private List<QuantityMeasurementEntity> ExecuteSpList(
            string spName, Action<SqlCommand>? paramBinder)
        {
            var results = new List<QuantityMeasurementEntity>();
            var conn    = _pool.GetConnection();
            SqlCommand? cmd = null; SqlDataReader? reader = null;
            try
            {
                cmd = new SqlCommand(spName, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                paramBinder?.Invoke(cmd);
                reader = cmd.ExecuteReader();
                while (reader.Read()) results.Add(MapRow(reader));
                return results;
            }
            catch (DatabaseException) { throw; }
            catch (Exception ex) { throw DatabaseException.QueryFailed(spName, ex); }
            finally { CloseResources(reader, cmd, conn); }
        }

        private static QuantityMeasurementEntity MapRow(SqlDataReader rs)
        {
            long     id           = rs.GetInt64(rs.GetOrdinal("id"));
            bool     hasError     = rs.GetBoolean(rs.GetOrdinal("has_error"));
            bool     isComparison = rs.GetBoolean(rs.GetOrdinal("is_comparison"));
            bool     compResult   = rs.GetBoolean(rs.GetOrdinal("comparison_result"));
            string   opType       = rs.GetString(rs.GetOrdinal("operation_type"));
            DateTime createdAt    = rs.GetDateTime(rs.GetOrdinal("created_at"));
            double   resultVal    = rs.IsDBNull(rs.GetOrdinal("result_value"))  ? 0    : rs.GetDouble(rs.GetOrdinal("result_value"));
            string?  resultUnit   = rs.IsDBNull(rs.GetOrdinal("result_unit"))   ? null : rs.GetString(rs.GetOrdinal("result_unit"));
            string?  errorMsg     = rs.IsDBNull(rs.GetOrdinal("error_message")) ? null : rs.GetString(rs.GetOrdinal("error_message"));

            var op1 = new QuantityDTO(
                rs.GetDouble(rs.GetOrdinal("operand1_value")),
                rs.GetString(rs.GetOrdinal("operand1_unit")),
                rs.GetString(rs.GetOrdinal("operand1_category")));

            QuantityDTO? op2 = null;
            if (!rs.IsDBNull(rs.GetOrdinal("operand2_value")))
                op2 = new QuantityDTO(
                    rs.GetDouble(rs.GetOrdinal("operand2_value")),
                    rs.IsDBNull(rs.GetOrdinal("operand2_unit"))     ? "" : rs.GetString(rs.GetOrdinal("operand2_unit")),
                    rs.IsDBNull(rs.GetOrdinal("operand2_category")) ? "" : rs.GetString(rs.GetOrdinal("operand2_category")));

            return new QuantityMeasurementEntity(
                id, op1, op2, opType, resultVal, resultUnit,
                isComparison, compResult, hasError, errorMsg, createdAt);
        }

        private void CloseResources(SqlDataReader? reader, SqlCommand? cmd, SqlConnection conn)
        {
            try { reader?.Close(); } catch { }
            try { cmd?.Dispose();  } catch { }
            _pool.ReleaseConnection(conn);
        }
    }
}
