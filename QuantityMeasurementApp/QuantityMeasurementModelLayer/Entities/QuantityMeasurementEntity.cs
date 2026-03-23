using System;
using QuantityMeasurementModelLayer.DTOs;

namespace QuantityMeasurementModelLayer.Entities
{
    public class QuantityMeasurementEntity
    {
        private QuantityDTO _operand1;
        private QuantityDTO _operand2;
        private string _operationType;
        private double _resultValue;
        private string _resultUnit;
        private bool _isComparison;
        private bool _comparisonResult;
        private bool _hasError;
        private string _errorMessage;
        private DateTime _createdAt;

        public QuantityDTO Operand1
        {
            get { return _operand1; }
        }

        public QuantityDTO Operand2
        {
            get { return _operand2; }
        }

        public string OperationType
        {
            get { return _operationType; }
        }

        public double ResultValue
        {
            get { return _resultValue; }
        }

        public string ResultUnit
        {
            get { return _resultUnit; }
        }

        public bool IsComparison
        {
            get { return _isComparison; }
        }

        public bool ComparisonResult
        {
            get { return _comparisonResult; }
        }

        public bool HasError
        {
            get { return _hasError; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
        }

        // Single-operand constructor (Convert)
        public QuantityMeasurementEntity(QuantityDTO operand1, string operationType,
                                         double resultValue, string resultUnit)
        {
            _operand1      = operand1;
            _operationType = operationType;
            _resultValue   = resultValue;
            _resultUnit    = resultUnit;
            _isComparison  = false;
            _hasError      = false;
            _createdAt     = DateTime.UtcNow;
        }

        // Binary-operand constructor (Add, Subtract, Divide)
        public QuantityMeasurementEntity(QuantityDTO operand1, QuantityDTO operand2,
                                         string operationType, double resultValue, string resultUnit)
        {
            _operand1      = operand1;
            _operand2      = operand2;
            _operationType = operationType;
            _resultValue   = resultValue;
            _resultUnit    = resultUnit;
            _isComparison  = false;
            _hasError      = false;
            _createdAt     = DateTime.UtcNow;
        }

        // Comparison constructor
        public QuantityMeasurementEntity(QuantityDTO operand1, QuantityDTO operand2,
                                         bool comparisonResult)
        {
            _operand1         = operand1;
            _operand2         = operand2;
            _operationType    = "COMPARE";
            _comparisonResult = comparisonResult;
            _isComparison     = true;
            _hasError         = false;
            _createdAt        = DateTime.UtcNow;
        }

        // Error constructor
        public QuantityMeasurementEntity(string operationType, string errorMessage)
        {
            _operationType = operationType;
            _errorMessage  = errorMessage;
            _hasError      = true;
            _createdAt     = DateTime.UtcNow;
        }

        public override string ToString()
        {
            if (_hasError)
            {
                return "[ERROR] " + _operationType + ": " + _errorMessage;
            }

            if (_isComparison)
            {
                return "[COMPARE] " + _operand1.ToString() + " == "
                       + _operand2.ToString() + " ? " + _comparisonResult;
            }

            if (_operand2 != null)
            {
                return "[" + _operationType + "] " + _operand1.ToString()
                       + " and " + _operand2.ToString()
                       + " = " + _resultValue + " " + _resultUnit;
            }

            return "[" + _operationType + "] " + _operand1.ToString()
                   + " => " + _resultValue + " " + _resultUnit;
        }
    }
}
