-- ============================================================
-- UC16: Quantity Measurement - Full Database Setup
-- Run this entire script in SSMS
-- ============================================================

-- Step 1: Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'QuantityMeasurementDB')
BEGIN
    CREATE DATABASE QuantityMeasurementDB;
    PRINT 'Database created.';
END
GO

USE QuantityMeasurementDB;
GO

-- Step 2: Create table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'quantity_measurement_entity')
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
        CONSTRAINT PK_quantity_measurement_entity PRIMARY KEY (id)
    );
    PRINT 'Table created.';
END
GO

-- Step 3: Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_operation_type' AND object_id = OBJECT_ID('quantity_measurement_entity'))
    CREATE INDEX idx_operation_type    ON quantity_measurement_entity(operation_type);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_operand1_category' AND object_id = OBJECT_ID('quantity_measurement_entity'))
    CREATE INDEX idx_operand1_category ON quantity_measurement_entity(operand1_category);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_created_at' AND object_id = OBJECT_ID('quantity_measurement_entity'))
    CREATE INDEX idx_created_at        ON quantity_measurement_entity(created_at);
GO

-- ============================================================
-- Step 4: Stored Procedures
-- ============================================================

-- SP: Save a measurement
IF OBJECT_ID('sp_SaveMeasurement', 'P') IS NOT NULL DROP PROCEDURE sp_SaveMeasurement;
GO
CREATE PROCEDURE sp_SaveMeasurement
    @operand1_value     FLOAT,
    @operand1_unit      NVARCHAR(50),
    @operand1_category  NVARCHAR(50),
    @operand2_value     FLOAT           = NULL,
    @operand2_unit      NVARCHAR(50)    = NULL,
    @operand2_category  NVARCHAR(50)    = NULL,
    @operation_type     NVARCHAR(20),
    @result_value       FLOAT           = NULL,
    @result_unit        NVARCHAR(50)    = NULL,
    @is_comparison      BIT             = 0,
    @comparison_result  BIT             = 0,
    @has_error          BIT             = 0,
    @error_message      NVARCHAR(500)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO quantity_measurement_entity
        (operand1_value, operand1_unit, operand1_category,
         operand2_value, operand2_unit, operand2_category,
         operation_type, result_value, result_unit,
         is_comparison, comparison_result,
         has_error, error_message, created_at)
    VALUES
        (@operand1_value, @operand1_unit, @operand1_category,
         @operand2_value, @operand2_unit, @operand2_category,
         @operation_type, @result_value, @result_unit,
         @is_comparison, @comparison_result,
         @has_error, @error_message, GETUTCDATE());

    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

-- SP: Get all measurements
IF OBJECT_ID('sp_GetAllMeasurements', 'P') IS NOT NULL DROP PROCEDURE sp_GetAllMeasurements;
GO
CREATE PROCEDURE sp_GetAllMeasurements
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM quantity_measurement_entity ORDER BY created_at DESC;
END
GO

-- SP: Get by ID
IF OBJECT_ID('sp_GetMeasurementById', 'P') IS NOT NULL DROP PROCEDURE sp_GetMeasurementById;
GO
CREATE PROCEDURE sp_GetMeasurementById
    @id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM quantity_measurement_entity WHERE id = @id;
END
GO

-- SP: Get by operation type
IF OBJECT_ID('sp_GetMeasurementsByOperation', 'P') IS NOT NULL DROP PROCEDURE sp_GetMeasurementsByOperation;
GO
CREATE PROCEDURE sp_GetMeasurementsByOperation
    @operation_type NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM quantity_measurement_entity
    WHERE operation_type = @operation_type
    ORDER BY created_at DESC;
END
GO

-- SP: Get by category
IF OBJECT_ID('sp_GetMeasurementsByType', 'P') IS NOT NULL DROP PROCEDURE sp_GetMeasurementsByType;
GO
CREATE PROCEDURE sp_GetMeasurementsByType
    @category NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM quantity_measurement_entity
    WHERE operand1_category = @category
    ORDER BY created_at DESC;
END
GO

-- SP: Get total count
IF OBJECT_ID('sp_GetTotalCount', 'P') IS NOT NULL DROP PROCEDURE sp_GetTotalCount;
GO
CREATE PROCEDURE sp_GetTotalCount
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS TotalCount FROM quantity_measurement_entity;
END
GO

-- SP: Delete by ID
IF OBJECT_ID('sp_DeleteMeasurement', 'P') IS NOT NULL DROP PROCEDURE sp_DeleteMeasurement;
GO
CREATE PROCEDURE sp_DeleteMeasurement
    @id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM quantity_measurement_entity WHERE id = @id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Delete all
IF OBJECT_ID('sp_DeleteAllMeasurements', 'P') IS NOT NULL DROP PROCEDURE sp_DeleteAllMeasurements;
GO
CREATE PROCEDURE sp_DeleteAllMeasurements
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM quantity_measurement_entity;
END
GO

PRINT 'All stored procedures created.';
PRINT 'Setup complete!';
GO
