using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuantityMeasurementApi.Dtos
{
    public class QuantityInputDto
    {
        [Required]
        [JsonPropertyName("thisQuantityDTO")]
        public QuantityDto? ThisQuantityDto { get; set; }

        [Required]
        [JsonPropertyName("thatQuantityDTO")]
        public QuantityDto? ThatQuantityDto { get; set; }
    }
}

