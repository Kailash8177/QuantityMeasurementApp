namespace QuantityMeasurementModelLayer.DTOs
{
    public class QuantityDTO
    {
        private double _value;
        private string _unit;
        private string _category;

        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public QuantityDTO()
        {
        }

        public QuantityDTO(double value, string unit, string category)
        {
            _value    = value;
            _unit     = unit;
            _category = category;
        }

        public override string ToString()
        {
            return _value + " " + _unit + " [" + _category + "]";
        }
    }
}
