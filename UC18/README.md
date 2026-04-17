# UC16 — Quantity Measurement with SQL Server Persistence (C# / .NET 8)

## Overview

This is the C# / .NET 8 implementation of **UC16: Database Integration for Quantity Measurement Persistence**.
It is a direct translation of the Java/Maven/JDBC design from the UC16 specification into idiomatic C# using:

| Java / UC16 | C# / UC16 |
|---|---|
| Maven project structure | .NET 8 multi-project solution |
| JDBC `PreparedStatement` | `SqlCommand` with `Parameters.AddWithValue` |
| `HikariCP` connection pool | Custom `ConnectionPool` (wraps `SqlConnection`) |
| `ApplicationConfig.java` | `DatabaseConfig.cs` (reads `appsettings.ini`) |
| `DatabaseException.java` | `DatabaseException.cs` (extends `QuantityMeasurementException`) |
| H2 / MySQL | SQL Server (SSMS) |
| `application.properties` | `appsettings.json` |
| JUnit 4 + Mockito | MSTest + Moq |
| `mvn clean compile` | `dotnet build` |
| `mvn test` | `dotnet test` |
| `mvn package` | `dotnet publish` |

---

## Project Structure

```
UC16Solution/
├── UC16Solution.sln                          # Solution file
├── appsettings.ini                           # DB config (edit this first)
├── Database/
│   └── schema.sql                            # SQL Server schema
├── QuantityMeasurementModelLayer/            # DTOs, Entities, Enums
├── QuantityMeasurementbusinessLayer/         # Service, Quantity<T>, IMeasurable impls
├── QuantityMeasurementRepositoryLayer/       # Cache + Database repositories, pool
│   ├── Util/
│   │   ├── DatabaseConfig.cs                 # ← ApplicationConfig.java equivalent
│   │   └── ConnectionPool.cs                 # ← HikariCP equivalent
│   └── Repositories/
│       ├── QuantityMeasurementCacheRepository.cs
│       └── QuantityMeasurementDatabaseRepository.cs   # ← UC16 JDBC repo
└── QuantityMeasurementApp/                   # Console app + controller + menu
    └── QuantityMeasurementApp.Tests/         # MSTest suite (80+ test cases)
```

---

## Prerequisites

- .NET 8 SDK (`dotnet --version` ≥ 8.0)
- SQL Server (local or remote) — SQL Server Express works fine
- SSMS for running `schema.sql`

---

## Step 1 — Configure your database

Edit `appsettings.json` and update the `Database.ConnectionString` for your SQL Server:

```json
{
  "Repository": { "Type": "database" },
  "Database": {
    "ConnectionString": "Server=localhost;Database=QuantityMeasurementDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

Common connection string variants:
```
// Windows Integrated Security (most common):
Server=localhost;Database=QuantityMeasurementDB;Integrated Security=True;TrustServerCertificate=True;

// SQL auth:
Server=localhost;Database=QuantityMeasurementDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;

// Named instance (SQL Express):
Server=localhost\SQLEXPRESS;Database=QuantityMeasurementDB;Integrated Security=True;TrustServerCertificate=True;
```

To use in-memory cache (no database required), set `"Repository": { "Type": "cache" }` in `appsettings.json`.

---

## Step 2 — Create the database schema

In SSMS, create the database first:
```sql
CREATE DATABASE QuantityMeasurementDB;
GO
```

Then run `Database/schema.sql` against `QuantityMeasurementDB`.  
(The `QuantityMeasurementDatabaseRepository` also auto-creates the table on first run.)

---

## Step 3 — Build

```bash
# Restore + build all projects
dotnet build UC16Solution.sln

# Or build individual project
dotnet build QuantityMeasurementApp/QuantityMeasurementApp.csproj
```

---

## Step 4 — Run

```bash
# Interactive console menu
dotnet run --project QuantityMeasurementApp/QuantityMeasurementApp.csproj

# Demo mode (like UC16 main() method)
dotnet run --project QuantityMeasurementApp/QuantityMeasurementApp.csproj -- --demo
```

---

## Step 5 — Test

```bash
# Run all tests
dotnet test UC16Solution.sln

# Run specific test class
dotnet test --filter "FullyQualifiedName~QuantityMeasurementServiceTest"

# Run integration tests only
dotnet test --filter "FullyQualifiedName~QuantityMeasurementIntegrationTest"

# Run UC15 backward-compatibility tests
dotnet test --filter "FullyQualifiedName~UC15BackwardCompatibilityTest"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

---

## Test Categories

| Test Class | UC16 Equivalent | Requires DB |
|---|---|---|
| `QuantityMeasurementEntityTest` | Entity layer | No |
| `QuantityDTOTest` | DTO layer | No |
| `MeasurableTest` | Unit implementations | No |
| `QuantityGenericTest` | `Quantity<U>` arithmetic | No |
| `ExceptionTest` | Exception hierarchy | No |
| `QuantityMeasurementCacheRepositoryTest` | Repository layer (cache) | No |
| `QuantityMeasurementServiceTest` | Service layer | No |
| `QuantityMeasurementControllerTest` | Controller layer | No |
| `DatabaseConfigTest` | Configuration loading | No |
| `SqlInjectionPreventionTest` | Security | No |
| `RepositoryTypeSwitchingTest` | DI / swapping repos | No |
| `ConcurrencyTest` | Thread safety | No |
| `PerformanceTest` | 100 entities performance | No |
| `QuantityMeasurementIntegrationTest` | End-to-end (uses cache) | No |
| `UC15BackwardCompatibilityTest` | All UC15 cases pass | No |

All tests listed above run without a database connection. To run database-specific
tests against a live SQL Server instance, see the commented-out notes in
`QuantityMeasurementDatabaseRepository.cs`.

---

## Key Design Concepts Demonstrated

1. **N-Tier architecture** — Model → Business → Repository → App, each independent
2. **Dependency injection** — `IQuantityMeasurementRepository` injected into service;
   swap cache ↔ database with zero service logic changes
3. **Connection pooling** — `ConnectionPool` singleton with acquire/release/validate
4. **Parameterized SQL** — all queries use `SqlCommand.Parameters` (SQL injection prevention)
5. **Configuration management** — `DatabaseConfig` reads `appsettings.ini` with env overrides
6. **Custom exception hierarchy** — `DatabaseException` extends `QuantityMeasurementException`
7. **Auto schema creation** — `QuantityMeasurementDatabaseRepository` creates the table on first run
8. **Transaction safety** — each CRUD operation gets its own connection from the pool
9. **Resource cleanup** — `CloseResources()` in both repository and `CloseAll()` in pool
10. **Backward compatibility** — all UC15 test cases pass unchanged

---

## Switching Between Cache and Database at Runtime

The repository type is selected from `appsettings.ini` in `QuantityMeasurementApp`:

```csharp
if (repoType == "database")
    _repository = QuantityMeasurementDatabaseRepository.GetInstance();
else
    _repository = QuantityMeasurementCacheRepository.GetInstance();
```

You can also override via environment variable before launching:
```bash
# PowerShell
$env:APP_ENV = "testing"
dotnet run --project QuantityMeasurementApp
```

---

## SQL Queries Used

```sql
-- Save a measurement
INSERT INTO quantity_measurement_entity
    (this_value, this_unit, this_measurement_type, ...)
OUTPUT INSERTED.id
VALUES (@thisValue, @thisUnit, @thisMeasurementType, ...)

-- Get all (most recent first)
SELECT * FROM quantity_measurement_entity ORDER BY created_at DESC

-- Filter by operation type
SELECT * FROM quantity_measurement_entity
WHERE operation = @operation ORDER BY created_at DESC

-- Filter by measurement category
SELECT * FROM quantity_measurement_entity
WHERE this_measurement_type = @measurementType ORDER BY created_at DESC

-- Count
SELECT COUNT(*) FROM quantity_measurement_entity

-- Delete all
DELETE FROM quantity_measurement_entity
```
