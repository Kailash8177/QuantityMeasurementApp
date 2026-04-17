using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace QuantityMeasurementRepositoryLayer.Util
{
    /// <summary>
    /// Loads database and application configuration from appsettings.json.
    /// Mirrors ApplicationConfig.java from UC16.
    /// </summary>
    public sealed class DatabaseConfig
    {
        // ── Singleton ─────────────────────────────────────────────────
        private static DatabaseConfig? _instance;
        private static readonly object _lock = new object();

        // ── Properties ────────────────────────────────────────────────
        public string RepositoryType     { get; private set; } = "database";
        public string AppEnvironment     { get; private set; } = "development";
        public string ConnectionString   { get; private set; } =
            "Server=localhost;Database=QuantityMeasurementDB;Integrated Security=True;TrustServerCertificate=True;";

        // Pool settings
        public int    MaxPoolSize         { get; private set; } = 10;
        public int    MinIdle             { get; private set; } = 2;
        public int    ConnectionTimeout   { get; private set; } = 30000;
        public int    IdleTimeout         { get; private set; } = 600000;
        public int    MaxLifetime         { get; private set; } = 1800000;
        public string PoolName            { get; private set; } = "QuantityMeasurementPool";
        public string PoolTestQuery       { get; private set; } = "SELECT 1";

        // Logging settings
        public string LoggingLevelRoot    { get; private set; } = "INFO";
        public string LoggingLevelApp     { get; private set; } = "DEBUG";

        // ── Private constructor ───────────────────────────────────────
        private DatabaseConfig() => LoadConfiguration();

        // ── Singleton factory ─────────────────────────────────────────
        public static DatabaseConfig GetInstance()
        {
            if (_instance == null)
                lock (_lock)
                    if (_instance == null)
                        _instance = new DatabaseConfig();
            return _instance;
        }

        // ── Load ──────────────────────────────────────────────────────

        private void LoadConfiguration()
        {
            string? filePath = ResolveFilePath("appsettings.json");

            if (filePath == null)
            {
                Console.WriteLine("[DatabaseConfig] appsettings.json not found. Using defaults.");
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // Repository.Type
                if (root.TryGetProperty("Repository", out var repo) &&
                    repo.TryGetProperty("Type", out var typeEl))
                    RepositoryType = typeEl.GetString() ?? RepositoryType;

                // App.Environment
                if (root.TryGetProperty("App", out var app) &&
                    app.TryGetProperty("Environment", out var envEl))
                    AppEnvironment = envEl.GetString() ?? AppEnvironment;

                // Database.ConnectionString
                if (root.TryGetProperty("Database", out var db))
                {
                    if (db.TryGetProperty("ConnectionString", out var csEl))
                        ConnectionString = csEl.GetString() ?? ConnectionString;

                    // Database.Pool.*
                    if (db.TryGetProperty("Pool", out var pool))
                    {
                        if (pool.TryGetProperty("MaxSize",           out var v) && v.TryGetInt32(out int i)) MaxPoolSize       = i;
                        if (pool.TryGetProperty("MinIdle",           out v)     && v.TryGetInt32(out i))     MinIdle           = i;
                        if (pool.TryGetProperty("ConnectionTimeout", out v)     && v.TryGetInt32(out i))     ConnectionTimeout = i;
                        if (pool.TryGetProperty("IdleTimeout",       out v)     && v.TryGetInt32(out i))     IdleTimeout       = i;
                        if (pool.TryGetProperty("MaxLifetime",       out v)     && v.TryGetInt32(out i))     MaxLifetime       = i;
                        if (pool.TryGetProperty("Name",              out v))                                 PoolName          = v.GetString() ?? PoolName;
                        if (pool.TryGetProperty("TestQuery",         out v))                                 PoolTestQuery     = v.GetString() ?? PoolTestQuery;
                    }
                }

                // Logging.Level.*
                if (root.TryGetProperty("Logging", out var logging) &&
                    logging.TryGetProperty("Level", out var level))
                {
                    if (level.TryGetProperty("Root",                 out var r)) LoggingLevelRoot = r.GetString() ?? LoggingLevelRoot;
                    if (level.TryGetProperty("QuantityMeasurement",  out var a)) LoggingLevelApp  = a.GetString() ?? LoggingLevelApp;
                }

                Console.WriteLine($"[DatabaseConfig] Loaded from: {filePath}");
                Console.WriteLine($"[DatabaseConfig] Repository={RepositoryType}, Environment={AppEnvironment}");
                Console.WriteLine($"[DatabaseConfig] Pool: max={MaxPoolSize}, minIdle={MinIdle}, timeout={ConnectionTimeout}ms");
                Console.WriteLine($"[DatabaseConfig] Logging: root={LoggingLevelRoot}, app={LoggingLevelApp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseConfig] Warning: {ex.Message}. Using defaults.");
            }
        }

        // ── Helpers ───────────────────────────────────────────────────

        public string GetProperty(string key, string? defaultValue = null) =>
            key?.ToLower() switch
            {
                "repository.type"              => RepositoryType,
                "app.env"                      => AppEnvironment,
                "db.connectionstring"          => ConnectionString,
                "db.pool.max-size"             => MaxPoolSize.ToString(),
                "db.pool.min-idle"             => MinIdle.ToString(),
                "db.pool.connection-timeout"   => ConnectionTimeout.ToString(),
                "db.pool.idle-timeout"         => IdleTimeout.ToString(),
                "db.pool.max-lifetime"         => MaxLifetime.ToString(),
                "db.pool.name"                 => PoolName,
                "db.pool.test-query"           => PoolTestQuery,
                "logging.level.root"           => LoggingLevelRoot,
                "logging.level.quantitymeasurement" => LoggingLevelApp,
                _ => defaultValue
            };

        public int GetIntProperty(string key, int defaultValue = 0)
        {
            string? v = GetProperty(key);
            return int.TryParse(v, out int result) ? result : defaultValue;
        }

        private static string? ResolveFilePath(string fileName)
        {
            string assemblyDir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) ?? ".";
            string path = Path.Combine(assemblyDir, fileName);
            if (File.Exists(path)) return path;

            path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            if (File.Exists(path)) return path;

            string? dir = Directory.GetCurrentDirectory();
            for (int i = 0; i < 4; i++)
            {
                dir = Path.GetDirectoryName(dir);
                if (dir == null) break;
                path = Path.Combine(dir, fileName);
                if (File.Exists(path)) return path;
            }

            return null;
        }
    }
}