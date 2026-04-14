using System.Text.Json.Serialization;
using QuantityMeasurementModelLayer.Entities;

namespace QuantityMeasurementApi.Dtos
{
    public class QuantityMeasurementDto
    {
        [JsonPropertyName("thisValue")]
        public double ThisValue { get; set; }

        [JsonPropertyName("thisUnit")]
        public string? ThisUnit { get; set; }

        [JsonPropertyName("thisMeasurementType")]
        public string? ThisMeasurementType { get; set; }

        [JsonPropertyName("thatValue")]
        public double ThatValue { get; set; }

        [JsonPropertyName("thatUnit")]
        public string? ThatUnit { get; set; }

        [JsonPropertyName("thatMeasurementType")]
        public string? ThatMeasurementType { get; set; }

        [JsonPropertyName("operation")]
        public string? Operation { get; set; }

        [JsonPropertyName("resultString")]
        public string? ResultString { get; set; }

        [JsonPropertyName("resultValue")]
        public double ResultValue { get; set; }

        [JsonPropertyName("resultUnit")]
        public string? ResultUnit { get; set; }

        [JsonPropertyName("resultMeasurementType")]
        public string? ResultMeasurementType { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        public static QuantityMeasurementDto FromEntity(QuantityMeasurementEntity e)
        {
            var dto = new QuantityMeasurementDto
            {
                ThisValue = e.Operand1?.Value ?? 0,
                ThisUnit = e.Operand1?.Unit,
                ThisMeasurementType = e.Operand1?.Category,
                ThatValue = e.Operand2?.Value ?? 0,
                ThatUnit = e.Operand2?.Unit,
                ThatMeasurementType = e.Operand2?.Category,
                Operation = e.OperationType?.ToLowerInvariant(),
                ResultValue = e.ResultValue,
                ResultUnit = e.ResultUnit,
                ResultMeasurementType = e.Operand1?.Category,
                ErrorMessage = e.ErrorMessage,
                Error = e.HasError
            };

            if (e.IsComparison)
                dto.ResultString = e.ComparisonResult.ToString().ToLowerInvariant();

            return dto;
        }
    }
}

