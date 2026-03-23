using System;
using QuantityMeasurementbusinessLayer.Implementations;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementModelLayer.Enums;
using QuantityMeasurementRepositoryLayer.Interfaces;

namespace QuantityMeasurementbusinessLayer
{
    public class QuantityMeasurementServiceImpl : IQuantityMeasurementService
    {
        private IQuantityMeasurementRepository _repository;

        public QuantityMeasurementServiceImpl(IQuantityMeasurementRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository cannot be null");
            }
            _repository = repository;
        }

        // ============================================================
        // Compare
        // ============================================================

        public QuantityMeasurementEntity Compare(QuantityDTO dto1, QuantityDTO dto2)
        {
            try
            {
                ValidateDTO(dto1, "dto1");
                ValidateDTO(dto2, "dto2");
                ValidateSameCategory(dto1, dto2, "COMPARE");

                bool result = CompareByCategory(dto1, dto2);

                QuantityMeasurementEntity entity =
                    new QuantityMeasurementEntity(dto1, dto2, result);

                return _repository.Save(entity);
            }
            catch (QuantityMeasurementException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException("Compare failed: " + ex.Message, ex);
            }
        }

        // ============================================================
        // Convert
        // ============================================================

        public QuantityMeasurementEntity Convert(QuantityDTO dto, string targetUnit)
        {
            try
            {
                ValidateDTO(dto, "dto");

                if (targetUnit == null || targetUnit.Trim().Length == 0)
                {
                    throw new QuantityMeasurementException("Target unit cannot be empty");
                }

                string target   = targetUnit.Trim().ToUpper();
                string category = dto.Category.ToLower();
                double resultValue;
                string resultUnitStr;

                if (category == "length")
                {
                    LengthUnit fromUnit     = ParseLengthUnit(dto.Unit);
                    LengthUnit toUnit       = ParseLengthUnit(target);
                    LengthMeasurable fromM  = new LengthMeasurable(fromUnit);
                    LengthMeasurable toM    = new LengthMeasurable(toUnit);
                    Quantity<LengthMeasurable> q = new Quantity<LengthMeasurable>(dto.Value, fromM);
                    Quantity<LengthMeasurable> r = q.ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "weight")
                {
                    WeightUnit fromUnit     = ParseWeightUnit(dto.Unit);
                    WeightUnit toUnit       = ParseWeightUnit(target);
                    WeightMeasurable fromM  = new WeightMeasurable(fromUnit);
                    WeightMeasurable toM    = new WeightMeasurable(toUnit);
                    Quantity<WeightMeasurable> q = new Quantity<WeightMeasurable>(dto.Value, fromM);
                    Quantity<WeightMeasurable> r = q.ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "volume")
                {
                    VolumeUnit fromUnit     = ParseVolumeUnit(dto.Unit);
                    VolumeUnit toUnit       = ParseVolumeUnit(target);
                    VolumeMeasurable fromM  = new VolumeMeasurable(fromUnit);
                    VolumeMeasurable toM    = new VolumeMeasurable(toUnit);
                    Quantity<VolumeMeasurable> q = new Quantity<VolumeMeasurable>(dto.Value, fromM);
                    Quantity<VolumeMeasurable> r = q.ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "temperature")
                {
                    TemperatureUnit fromUnit      = ParseTemperatureUnit(dto.Unit);
                    TemperatureUnit toUnit        = ParseTemperatureUnit(target);
                    TemperatureMeasurable fromM   = new TemperatureMeasurable(fromUnit);
                    TemperatureMeasurable toM     = new TemperatureMeasurable(toUnit);
                    Quantity<TemperatureMeasurable> q = new Quantity<TemperatureMeasurable>(dto.Value, fromM);
                    Quantity<TemperatureMeasurable> r = q.ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else
                {
                    throw new QuantityMeasurementException("Unknown category: " + dto.Category);
                }

                QuantityMeasurementEntity entity =
                    new QuantityMeasurementEntity(dto, "CONVERT", resultValue, resultUnitStr);

                return _repository.Save(entity);
            }
            catch (QuantityMeasurementException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException("Convert failed: " + ex.Message, ex);
            }
        }

        // ============================================================
        // Add
        // ============================================================

        public QuantityMeasurementEntity Add(QuantityDTO dto1, QuantityDTO dto2, string targetUnit)
        {
            try
            {
                ValidateDTO(dto1, "dto1");
                ValidateDTO(dto2, "dto2");
                ValidateSameCategory(dto1, dto2, "ADD");

                string target   = targetUnit.Trim().ToUpper();
                string category = dto1.Category.ToLower();
                double resultValue;
                string resultUnitStr;

                if (category == "length")
                {
                    LengthMeasurable m1  = new LengthMeasurable(ParseLengthUnit(dto1.Unit));
                    LengthMeasurable m2  = new LengthMeasurable(ParseLengthUnit(dto2.Unit));
                    LengthMeasurable tgt = new LengthMeasurable(ParseLengthUnit(target));
                    Quantity<LengthMeasurable> q1 = new Quantity<LengthMeasurable>(dto1.Value, m1);
                    Quantity<LengthMeasurable> q2 = new Quantity<LengthMeasurable>(dto2.Value, m2);
                    Quantity<LengthMeasurable> r  = q1.Add(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "weight")
                {
                    WeightMeasurable m1  = new WeightMeasurable(ParseWeightUnit(dto1.Unit));
                    WeightMeasurable m2  = new WeightMeasurable(ParseWeightUnit(dto2.Unit));
                    WeightMeasurable tgt = new WeightMeasurable(ParseWeightUnit(target));
                    Quantity<WeightMeasurable> q1 = new Quantity<WeightMeasurable>(dto1.Value, m1);
                    Quantity<WeightMeasurable> q2 = new Quantity<WeightMeasurable>(dto2.Value, m2);
                    Quantity<WeightMeasurable> r  = q1.Add(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "volume")
                {
                    VolumeMeasurable m1  = new VolumeMeasurable(ParseVolumeUnit(dto1.Unit));
                    VolumeMeasurable m2  = new VolumeMeasurable(ParseVolumeUnit(dto2.Unit));
                    VolumeMeasurable tgt = new VolumeMeasurable(ParseVolumeUnit(target));
                    Quantity<VolumeMeasurable> q1 = new Quantity<VolumeMeasurable>(dto1.Value, m1);
                    Quantity<VolumeMeasurable> q2 = new Quantity<VolumeMeasurable>(dto2.Value, m2);
                    Quantity<VolumeMeasurable> r  = q1.Add(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else
                {
                    // Temperature and unknown categories
                    TemperatureMeasurable m1 = new TemperatureMeasurable(ParseTemperatureUnit(dto1.Unit));
                    Quantity<TemperatureMeasurable> q1 = new Quantity<TemperatureMeasurable>(dto1.Value, m1);
                    TemperatureMeasurable m2 = new TemperatureMeasurable(ParseTemperatureUnit(dto2.Unit));
                    Quantity<TemperatureMeasurable> q2 = new Quantity<TemperatureMeasurable>(dto2.Value, m2);
                    // This will throw via ValidateOperationSupport inside Quantity.Add
                    TemperatureMeasurable tgt = new TemperatureMeasurable(ParseTemperatureUnit(target));
                    Quantity<TemperatureMeasurable> r  = q1.Add(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }

                QuantityMeasurementEntity entity =
                    new QuantityMeasurementEntity(dto1, dto2, "ADD", resultValue, resultUnitStr);

                return _repository.Save(entity);
            }
            catch (QuantityMeasurementException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new QuantityMeasurementException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException("Add failed: " + ex.Message, ex);
            }
        }

        // ============================================================
        // Subtract
        // ============================================================

        public QuantityMeasurementEntity Subtract(QuantityDTO dto1, QuantityDTO dto2, string targetUnit)
        {
            try
            {
                ValidateDTO(dto1, "dto1");
                ValidateDTO(dto2, "dto2");
                ValidateSameCategory(dto1, dto2, "SUBTRACT");

                string target   = targetUnit.Trim().ToUpper();
                string category = dto1.Category.ToLower();
                double resultValue;
                string resultUnitStr;

                if (category == "length")
                {
                    LengthMeasurable m1  = new LengthMeasurable(ParseLengthUnit(dto1.Unit));
                    LengthMeasurable m2  = new LengthMeasurable(ParseLengthUnit(dto2.Unit));
                    LengthMeasurable tgt = new LengthMeasurable(ParseLengthUnit(target));
                    Quantity<LengthMeasurable> q1 = new Quantity<LengthMeasurable>(dto1.Value, m1);
                    Quantity<LengthMeasurable> q2 = new Quantity<LengthMeasurable>(dto2.Value, m2);
                    Quantity<LengthMeasurable> r  = q1.Subtract(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "weight")
                {
                    WeightMeasurable m1  = new WeightMeasurable(ParseWeightUnit(dto1.Unit));
                    WeightMeasurable m2  = new WeightMeasurable(ParseWeightUnit(dto2.Unit));
                    WeightMeasurable tgt = new WeightMeasurable(ParseWeightUnit(target));
                    Quantity<WeightMeasurable> q1 = new Quantity<WeightMeasurable>(dto1.Value, m1);
                    Quantity<WeightMeasurable> q2 = new Quantity<WeightMeasurable>(dto2.Value, m2);
                    Quantity<WeightMeasurable> r  = q1.Subtract(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "volume")
                {
                    VolumeMeasurable m1  = new VolumeMeasurable(ParseVolumeUnit(dto1.Unit));
                    VolumeMeasurable m2  = new VolumeMeasurable(ParseVolumeUnit(dto2.Unit));
                    VolumeMeasurable tgt = new VolumeMeasurable(ParseVolumeUnit(target));
                    Quantity<VolumeMeasurable> q1 = new Quantity<VolumeMeasurable>(dto1.Value, m1);
                    Quantity<VolumeMeasurable> q2 = new Quantity<VolumeMeasurable>(dto2.Value, m2);
                    Quantity<VolumeMeasurable> r  = q1.Subtract(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else
                {
                    TemperatureMeasurable m1  = new TemperatureMeasurable(ParseTemperatureUnit(dto1.Unit));
                    TemperatureMeasurable m2  = new TemperatureMeasurable(ParseTemperatureUnit(dto2.Unit));
                    TemperatureMeasurable tgt = new TemperatureMeasurable(ParseTemperatureUnit(target));
                    Quantity<TemperatureMeasurable> q1 = new Quantity<TemperatureMeasurable>(dto1.Value, m1);
                    Quantity<TemperatureMeasurable> q2 = new Quantity<TemperatureMeasurable>(dto2.Value, m2);
                    Quantity<TemperatureMeasurable> r  = q1.Subtract(q2, tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }

                QuantityMeasurementEntity entity =
                    new QuantityMeasurementEntity(dto1, dto2, "SUBTRACT", resultValue, resultUnitStr);

                return _repository.Save(entity);
            }
            catch (QuantityMeasurementException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new QuantityMeasurementException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException("Subtract failed: " + ex.Message, ex);
            }
        }

        // ============================================================
        // Divide
        // ============================================================

        public QuantityMeasurementEntity Divide(QuantityDTO dto1, QuantityDTO dto2)
        {
            try
            {
                ValidateDTO(dto1, "dto1");
                ValidateDTO(dto2, "dto2");
                ValidateSameCategory(dto1, dto2, "DIVIDE");

                string category = dto1.Category.ToLower();
                double scalar;

                if (category == "length")
                {
                    LengthMeasurable m1 = new LengthMeasurable(ParseLengthUnit(dto1.Unit));
                    LengthMeasurable m2 = new LengthMeasurable(ParseLengthUnit(dto2.Unit));
                    Quantity<LengthMeasurable> q1 = new Quantity<LengthMeasurable>(dto1.Value, m1);
                    Quantity<LengthMeasurable> q2 = new Quantity<LengthMeasurable>(dto2.Value, m2);
                    scalar = q1.Divide(q2);
                }
                else if (category == "weight")
                {
                    WeightMeasurable m1 = new WeightMeasurable(ParseWeightUnit(dto1.Unit));
                    WeightMeasurable m2 = new WeightMeasurable(ParseWeightUnit(dto2.Unit));
                    Quantity<WeightMeasurable> q1 = new Quantity<WeightMeasurable>(dto1.Value, m1);
                    Quantity<WeightMeasurable> q2 = new Quantity<WeightMeasurable>(dto2.Value, m2);
                    scalar = q1.Divide(q2);
                }
                else if (category == "volume")
                {
                    VolumeMeasurable m1 = new VolumeMeasurable(ParseVolumeUnit(dto1.Unit));
                    VolumeMeasurable m2 = new VolumeMeasurable(ParseVolumeUnit(dto2.Unit));
                    Quantity<VolumeMeasurable> q1 = new Quantity<VolumeMeasurable>(dto1.Value, m1);
                    Quantity<VolumeMeasurable> q2 = new Quantity<VolumeMeasurable>(dto2.Value, m2);
                    scalar = q1.Divide(q2);
                }
                else
                {
                    TemperatureMeasurable m1 = new TemperatureMeasurable(ParseTemperatureUnit(dto1.Unit));
                    TemperatureMeasurable m2 = new TemperatureMeasurable(ParseTemperatureUnit(dto2.Unit));
                    Quantity<TemperatureMeasurable> q1 = new Quantity<TemperatureMeasurable>(dto1.Value, m1);
                    Quantity<TemperatureMeasurable> q2 = new Quantity<TemperatureMeasurable>(dto2.Value, m2);
                    scalar = q1.Divide(q2);
                }

                QuantityMeasurementEntity entity =
                    new QuantityMeasurementEntity(dto1, dto2, "DIVIDE", scalar, "scalar");

                return _repository.Save(entity);
            }
            catch (QuantityMeasurementException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new QuantityMeasurementException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException("Divide failed: " + ex.Message, ex);
            }
        }

        // ============================================================
        // Private Validation
        // ============================================================

        private static void ValidateDTO(QuantityDTO dto, string name)
        {
            if (dto == null)
            {
                throw new QuantityMeasurementException(name + " cannot be null");
            }
            if (dto.Unit == null || dto.Unit.Trim().Length == 0)
            {
                throw new QuantityMeasurementException(name + ".Unit cannot be empty");
            }
            if (dto.Category == null || dto.Category.Trim().Length == 0)
            {
                throw new QuantityMeasurementException(name + ".Category cannot be empty");
            }
        }

        private static void ValidateSameCategory(QuantityDTO a, QuantityDTO b, string op)
        {
            if (string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new QuantityMeasurementException(
                    op + ": Cannot mix categories '"
                    + a.Category + "' and '" + b.Category + "'");
            }
        }

        // ============================================================
        // Private Compare dispatcher
        // ============================================================

        private static bool CompareByCategory(QuantityDTO a, QuantityDTO b)
        {
            string category = a.Category.ToLower();

            if (category == "length")
            {
                LengthMeasurable m1 = new LengthMeasurable(ParseLengthUnit(a.Unit));
                LengthMeasurable m2 = new LengthMeasurable(ParseLengthUnit(b.Unit));
                Quantity<LengthMeasurable> q1 = new Quantity<LengthMeasurable>(a.Value, m1);
                Quantity<LengthMeasurable> q2 = new Quantity<LengthMeasurable>(b.Value, m2);
                return q1.Equals(q2);
            }

            if (category == "weight")
            {
                WeightMeasurable m1 = new WeightMeasurable(ParseWeightUnit(a.Unit));
                WeightMeasurable m2 = new WeightMeasurable(ParseWeightUnit(b.Unit));
                Quantity<WeightMeasurable> q1 = new Quantity<WeightMeasurable>(a.Value, m1);
                Quantity<WeightMeasurable> q2 = new Quantity<WeightMeasurable>(b.Value, m2);
                return q1.Equals(q2);
            }

            if (category == "volume")
            {
                VolumeMeasurable m1 = new VolumeMeasurable(ParseVolumeUnit(a.Unit));
                VolumeMeasurable m2 = new VolumeMeasurable(ParseVolumeUnit(b.Unit));
                Quantity<VolumeMeasurable> q1 = new Quantity<VolumeMeasurable>(a.Value, m1);
                Quantity<VolumeMeasurable> q2 = new Quantity<VolumeMeasurable>(b.Value, m2);
                return q1.Equals(q2);
            }

            if (category == "temperature")
            {
                TemperatureMeasurable m1 = new TemperatureMeasurable(ParseTemperatureUnit(a.Unit));
                TemperatureMeasurable m2 = new TemperatureMeasurable(ParseTemperatureUnit(b.Unit));
                Quantity<TemperatureMeasurable> q1 = new Quantity<TemperatureMeasurable>(a.Value, m1);
                Quantity<TemperatureMeasurable> q2 = new Quantity<TemperatureMeasurable>(b.Value, m2);
                return q1.Equals(q2);
            }

            throw new QuantityMeasurementException("Unknown category: " + a.Category);
        }

        // ============================================================
        // Private Unit Parsers
        // ============================================================

        private static LengthUnit ParseLengthUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            if (u == "FEET")        { return LengthUnit.FEET; }
            if (u == "INCHES")      { return LengthUnit.INCHES; }
            if (u == "YARDS")       { return LengthUnit.YARDS; }
            if (u == "CENTIMETERS") { return LengthUnit.CENTIMETERS; }
            throw new QuantityMeasurementException("Invalid LengthUnit: " + unit);
        }

        private static WeightUnit ParseWeightUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            if (u == "KILOGRAM") { return WeightUnit.KILOGRAM; }
            if (u == "GRAM")     { return WeightUnit.GRAM; }
            throw new QuantityMeasurementException("Invalid WeightUnit: " + unit);
        }

        private static VolumeUnit ParseVolumeUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            if (u == "LITRE")      { return VolumeUnit.LITRE; }
            if (u == "MILLILITRE") { return VolumeUnit.MILLILITRE; }
            if (u == "GALLON")     { return VolumeUnit.GALLON; }
            throw new QuantityMeasurementException("Invalid VolumeUnit: " + unit);
        }

        private static TemperatureUnit ParseTemperatureUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            if (u == "CELSIUS")    { return TemperatureUnit.CELSIUS; }
            if (u == "FAHRENHEIT") { return TemperatureUnit.FAHRENHEIT; }
            if (u == "KELVIN")     { return TemperatureUnit.KELVIN; }
            throw new QuantityMeasurementException("Invalid TemperatureUnit: " + unit);
        }
    }
}