using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuantityMeasurementApi.Dtos
{
    public class QuantityDto
    {
        [Required]
        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [Required]
        [MinLength(1)]
        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [Required]
        [MinLength(1)]
        [JsonPropertyName("measurementType")]
        public string? MeasurementType { get; set; }
    }
}

