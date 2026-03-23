using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QuantityMeasurementbusinessLayer;

namespace QuantityMeasurementRepositoryLayer.Util
{
    /// <summary>
    /// Manages a pool of SQL Server connections for reuse.
    /// Mirrors ConnectionPool.java from UC16.
    ///
    /// NOTE: In production .NET the SqlConnection class already benefits from
    /// ADO.NET's built-in connection pooling managed by the runtime.
    /// This class provides an explicit, observable pool so the concepts
    /// from UC16 (available/used counts, pool stats, acquire/release) are
    /// faithfully demonstrated. Acquire/release are synchronised.
    /// </summary>
    public sealed class ConnectionPool : IDisposable
    {
        // ── Singleton ─────────────────────────────────────────────────
        private static ConnectionPool _instance;
        private static readonly object _instanceLock = new object();

        // ── Pool state ────────────────────────────────────────────────
        private readonly List<SqlConnection> _available = new List<SqlConnection>();
        private readonly List<SqlConnection> _used      = new List<SqlConnection>();
        private readonly object _poolLock               = new object();

        private readonly int    _poolSize;
        private readonly string _connectionString;
        private readonly string _testQuery;

        // ── Private constructor ───────────────────────────────────────
        private ConnectionPool()
        {
            var config       = DatabaseConfig.GetInstance();
            _connectionString = config.ConnectionString;
            _poolSize         = config.MaxPoolSize;
            _testQuery        = config.PoolTestQuery;

            Console.WriteLine($"[ConnectionPool] Initialising pool '{config.PoolName}' " +
                              $"(size={_poolSize}, server={GetServerName()})");

            // Pre-create minimum idle connections
            int minIdle = config.MinIdle;
            for (int i = 0; i < minIdle; i++)
            {
                try { _available.Add(CreateConnection()); }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ConnectionPool] Warning: could not pre-create connection {i + 1}: {ex.Message}");
                }
            }
        }

        // ── Singleton factory ─────────────────────────────────────────
        public static ConnectionPool GetInstance()
        {
            if (_instance == null)
                lock (_instanceLock)
                    if (_instance == null)
                    {
                        try
                        {
                            _instance = new ConnectionPool();
                        }
                        catch (Exception)
                        {
                            _instance = null; // allow retry
                            throw;
                        }
                    }
            return _instance;
        }

        // ── Create ────────────────────────────────────────────────────

        private SqlConnection CreateConnection()
        {
            try
            {
                var conn = new SqlConnection(_connectionString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                throw DatabaseException.ConnectionFailed(
                    $"Could not open SQL Server connection: {ex.Message}", ex);
            }
        }

        // ── Acquire ───────────────────────────────────────────────────

        /// <summary>
        /// Returns a connection from the pool. Creates a new one if none
        /// are available and the pool has not reached its maximum size.
        /// </summary>
        public SqlConnection GetConnection()
        {
            lock (_poolLock)
            {
                // Reuse an available connection if one is healthy
                while (_available.Count > 0)
                {
                    var conn = _available[_available.Count - 1];
                    _available.RemoveAt(_available.Count - 1);

                    if (ValidateConnection(conn))
                    {
                        _used.Add(conn);
                        return conn;
                    }
                    else
                    {
                        TryClose(conn); // discard stale connection
                    }
                }

                // Create a new connection if pool not exhausted
                if (_used.Count < _poolSize)
                {
                    var conn = CreateConnection();
                    _used.Add(conn);
                    return conn;
                }

                throw new DatabaseException(
                    $"Connection pool exhausted (max={_poolSize}). " +
                    "All connections are in use.");
            }
        }

        // ── Release ───────────────────────────────────────────────────

        /// <summary>Returns a connection back to the available pool.</summary>
        public void ReleaseConnection(SqlConnection connection)
        {
            if (connection == null) return;

            lock (_poolLock)
            {
                _used.Remove(connection);

                if (connection.State == ConnectionState.Open)
                    _available.Add(connection);
                else
                    TryClose(connection);
            }
        }

        // ── Validation ────────────────────────────────────────────────

        public bool ValidateConnection(SqlConnection connection)
        {
            if (connection == null || connection.State != ConnectionState.Open) return false;
            try
            {
                using var cmd = new SqlCommand(_testQuery, connection);
                cmd.ExecuteScalar();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── Stats ─────────────────────────────────────────────────────

        public int AvailableCount => _available.Count;
        public int UsedCount      => _used.Count;
        public int TotalCount     => _available.Count + _used.Count;

        public string GetPoolStatistics() =>
            $"ConnectionPool[available={AvailableCount}, used={UsedCount}, total={TotalCount}, max={_poolSize}]";

        // ── Cleanup ───────────────────────────────────────────────────

        public void CloseAll()
        {
            lock (_poolLock)
            {
                foreach (var c in _available) TryClose(c);
                foreach (var c in _used)      TryClose(c);
                _available.Clear();
                _used.Clear();
            }
        }

        public void Dispose() => CloseAll();

        private static void TryClose(SqlConnection conn)
        {
            try { conn?.Close(); conn?.Dispose(); } catch { /* ignore */ }
        }

        private string GetServerName()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString);
                return builder.DataSource ?? "unknown";
            }
            catch { return "unknown"; }
        }

        public override string ToString() => GetPoolStatistics();
    }
}
