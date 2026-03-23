using System;

namespace QuantityMeasurementbusinessLayer
{
    /// <summary>Domain-level exception for all quantity measurement errors.</summary>
    public class QuantityMeasurementException : Exception
    {
        public QuantityMeasurementException(string message) : base(message) { }
        public QuantityMeasurementException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Database-specific exception. Extends QuantityMeasurementException so
    /// upper layers can catch either. Mirrors DatabaseException.java from UC16.
    /// </summary>
    public class DatabaseException : QuantityMeasurementException
    {
        public DatabaseException(string message) : base(message) { }
        public DatabaseException(string message, Exception inner) : base(message, inner) { }

        public static DatabaseException ConnectionFailed(string details, Exception cause) =>
            new DatabaseException($"Database connection failed: {details}", cause);

        public static DatabaseException QueryFailed(string query, Exception cause) =>
            new DatabaseException($"Query execution failed: {query}", cause);

        public static DatabaseException TransactionFailed(string operation, Exception cause) =>
            new DatabaseException($"Transaction failed during {operation}", cause);
    }
}
