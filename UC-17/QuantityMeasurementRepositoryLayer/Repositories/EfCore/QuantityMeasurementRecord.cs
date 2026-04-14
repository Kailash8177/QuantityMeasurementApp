using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantityMeasurementRepositoryLayer.Repositories.EfCore
{
    [Table("quantity_measurement_entity")]
    public class QuantityMeasurementRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("operand1_value")]
        public double Operand1Value { get; set; }

        [Column("operand1_unit")]
        [MaxLength(50)]
        public string Operand1Unit { get; set; } = string.Empty;

        [Column("operand1_category")]
        [MaxLength(50)]
        public string Operand1Category { get; set; } = string.Empty;

        [Column("operand2_value")]
        public double? Operand2Value { get; set; }

        [Column("operand2_unit")]
        [MaxLength(50)]
        public string? Operand2Unit { get; set; }

        [Column("operand2_category")]
        [MaxLength(50)]
        public string? Operand2Category { get; set; }

        [Column("operation_type")]
        [MaxLength(20)]
        public string OperationType { get; set; } = string.Empty;

        [Column("result_value")]
        public double? ResultValue { get; set; }

        [Column("result_unit")]
        [MaxLength(50)]
        public string? ResultUnit { get; set; }

        [Column("is_comparison")]
        public bool IsComparison { get; set; }

        [Column("comparison_result")]
        public bool ComparisonResult { get; set; }

        [Column("has_error")]
        public bool HasError { get; set; }

        [Column("error_message")]
        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

