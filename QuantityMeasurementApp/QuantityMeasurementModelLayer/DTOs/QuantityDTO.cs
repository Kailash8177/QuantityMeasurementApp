namespace QuantityMeasurementModelLayer.DTOs
{
    public class QuantityDTO
    {
        public double  Value    { get; set; }
        public string? Unit     { get; set; }
        public string? Category { get; set; }

        public QuantityDTO() { }

        public QuantityDTO(double value, string unit, string category)
        {
            Value    = value;
            Unit     = unit;
            Category = category;
        }

        public override string ToString() =>
            $"{Value} {Unit} [{Category}]";
    }
}
