using System;
using QuantityMeasurementModelLayer.DTOs;

namespace QuantityMeasurementModelLayer.Entities
{
    public class QuantityMeasurementEntity
    {
        public long         Id               { get; set; }
        public QuantityDTO? Operand1         { get; private set; }
        public QuantityDTO? Operand2         { get; private set; }
        public string       OperationType   { get; private set; } = string.Empty;
        public double       ResultValue     { get; private set; }
        public string?      ResultUnit      { get; private set; }
        public bool         IsComparison    { get; private set; }
        public bool         ComparisonResult { get; private set; }
        public bool         HasError        { get; private set; }
        public string?      ErrorMessage    { get; private set; }
        public DateTime     CreatedAt       { get; private set; }

        // Single-operand (Convert)
        public QuantityMeasurementEntity(
            QuantityDTO operand1, string operationType,
            double resultValue, string resultUnit)
        {
            Operand1      = operand1;
            OperationType = operationType;
            ResultValue   = resultValue;
            ResultUnit    = resultUnit;
            CreatedAt     = DateTime.UtcNow;
        }

        // Binary-operand (Add, Subtract, Divide)
        public QuantityMeasurementEntity(
            QuantityDTO operand1, QuantityDTO operand2,
            string operationType, double resultValue, string resultUnit)
        {
            Operand1      = operand1;
            Operand2      = operand2;
            OperationType = operationType;
            ResultValue   = resultValue;
            ResultUnit    = resultUnit;
            CreatedAt     = DateTime.UtcNow;
        }

        // Comparison
        public QuantityMeasurementEntity(
            QuantityDTO operand1, QuantityDTO operand2, bool comparisonResult)
        {
            Operand1         = operand1;
            Operand2         = operand2;
            OperationType    = "COMPARE";
            ComparisonResult = comparisonResult;
            IsComparison     = true;
            CreatedAt        = DateTime.UtcNow;
        }

        // Error
        public QuantityMeasurementEntity(string operationType, string errorMessage)
        {
            OperationType = operationType;
            ErrorMessage  = errorMessage;
            HasError      = true;
            CreatedAt     = DateTime.UtcNow;
        }

        // Reconstructed from database row
        public QuantityMeasurementEntity(
            long id,
            QuantityDTO? operand1, QuantityDTO? operand2,
            string operationType,
            double resultValue, string? resultUnit,
            bool isComparison, bool comparisonResult,
            bool hasError, string? errorMessage,
            DateTime createdAt)
        {
            Id               = id;
            Operand1         = operand1;
            Operand2         = operand2;
            OperationType    = operationType;
            ResultValue      = resultValue;
            ResultUnit       = resultUnit;
            IsComparison     = isComparison;
            ComparisonResult = comparisonResult;
            HasError         = hasError;
            ErrorMessage     = errorMessage;
            CreatedAt        = createdAt;
        }

        public override string ToString()
        {
            if (HasError)
                return $"[ERROR] {OperationType}: {ErrorMessage}";
            if (IsComparison)
                return $"[COMPARE] {Operand1} == {Operand2} ? {ComparisonResult}";
            if (Operand2 != null)
                return $"[{OperationType}] {Operand1} and {Operand2} = {ResultValue} {ResultUnit}";
            return $"[{OperationType}] {Operand1} => {ResultValue} {ResultUnit}";
        }
    }
}
