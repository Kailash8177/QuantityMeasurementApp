using System;
using QuantityMeasurementbusinessLayer.Implementations;
using QuantityMeasurementModelLayer.DTOs;
using QuantityMeasurementModelLayer.Entities;
using QuantityMeasurementModelLayer.Enums;

namespace QuantityMeasurementbusinessLayer
{
    /// <summary>
    /// Business logic for all quantity measurement operations.
    /// Accepts any IQuantityMeasurementRepository via constructor injection —
    /// swap the cache or the database repository without changing service logic.
    /// Mirrors QuantityMeasurementServiceImpl.java from UC16.
    /// </summary>
    public class QuantityMeasurementServiceImpl : IQuantityMeasurementService
    {
        private readonly IQuantityMeasurementRepository _repository;

        public QuantityMeasurementServiceImpl(IQuantityMeasurementRepository repository)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository),
                    "Repository cannot be null");

            Console.WriteLine($"[ServiceImpl] Initialised with repository: " +
                              $"{repository.GetType().Name}");
        }

        // ── Compare ───────────────────────────────────────────────────

        public QuantityMeasurementEntity Compare(QuantityDTO dto1, QuantityDTO dto2)
        {
            try
            {
                ValidateDTO(dto1, "dto1");
                ValidateDTO(dto2, "dto2");
                ValidateSameCategory(dto1, dto2, "COMPARE");

                bool result = CompareByCategory(dto1, dto2);
                var entity  = new QuantityMeasurementEntity(dto1, dto2, result);
                _repository.Save(entity);
                return entity;
            }
            catch (QuantityMeasurementException) { throw; }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException($"Compare failed: {ex.Message}", ex);
            }
        }

        // ── Convert ───────────────────────────────────────────────────

        public QuantityMeasurementEntity Convert(QuantityDTO dto, string targetUnit)
        {
            try
            {
                ValidateDTO(dto, "dto");
                if (string.IsNullOrWhiteSpace(targetUnit))
                    throw new QuantityMeasurementException("Target unit cannot be empty");

                string target   = targetUnit.Trim().ToUpper();
                string category = dto.Category.ToLower();
                double resultValue;
                string resultUnitStr;

                if (category == "length")
                {
                    var fromM = new LengthMeasurable(ParseLengthUnit(dto.Unit));
                    var toM   = new LengthMeasurable(ParseLengthUnit(target));
                    var r     = new Quantity<LengthMeasurable>(dto.Value, fromM).ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "weight")
                {
                    var fromM = new WeightMeasurable(ParseWeightUnit(dto.Unit));
                    var toM   = new WeightMeasurable(ParseWeightUnit(target));
                    var r     = new Quantity<WeightMeasurable>(dto.Value, fromM).ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "volume")
                {
                    var fromM = new VolumeMeasurable(ParseVolumeUnit(dto.Unit));
                    var toM   = new VolumeMeasurable(ParseVolumeUnit(target));
                    var r     = new Quantity<VolumeMeasurable>(dto.Value, fromM).ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "temperature")
                {
                    var fromM = new TemperatureMeasurable(ParseTemperatureUnit(dto.Unit));
                    var toM   = new TemperatureMeasurable(ParseTemperatureUnit(target));
                    var r     = new Quantity<TemperatureMeasurable>(dto.Value, fromM).ConvertTo(toM);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else throw new QuantityMeasurementException($"Unknown category: {dto.Category}");

                var entity = new QuantityMeasurementEntity(dto, "CONVERT", resultValue, resultUnitStr);
                _repository.Save(entity);
                return entity;
            }
            catch (QuantityMeasurementException) { throw; }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException($"Convert failed: {ex.Message}", ex);
            }
        }

        // ── Add ───────────────────────────────────────────────────────

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
                    var m1  = new LengthMeasurable(ParseLengthUnit(dto1.Unit));
                    var m2  = new LengthMeasurable(ParseLengthUnit(dto2.Unit));
                    var tgt = new LengthMeasurable(ParseLengthUnit(target));
                    var r   = new Quantity<LengthMeasurable>(dto1.Value, m1)
                                  .Add(new Quantity<LengthMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "weight")
                {
                    var m1  = new WeightMeasurable(ParseWeightUnit(dto1.Unit));
                    var m2  = new WeightMeasurable(ParseWeightUnit(dto2.Unit));
                    var tgt = new WeightMeasurable(ParseWeightUnit(target));
                    var r   = new Quantity<WeightMeasurable>(dto1.Value, m1)
                                  .Add(new Quantity<WeightMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "volume")
                {
                    var m1  = new VolumeMeasurable(ParseVolumeUnit(dto1.Unit));
                    var m2  = new VolumeMeasurable(ParseVolumeUnit(dto2.Unit));
                    var tgt = new VolumeMeasurable(ParseVolumeUnit(target));
                    var r   = new Quantity<VolumeMeasurable>(dto1.Value, m1)
                                  .Add(new Quantity<VolumeMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else
                {
                    // Temperature — will throw via ValidateOperationSupport
                    var m1  = new TemperatureMeasurable(ParseTemperatureUnit(dto1.Unit));
                    var m2  = new TemperatureMeasurable(ParseTemperatureUnit(dto2.Unit));
                    var tgt = new TemperatureMeasurable(ParseTemperatureUnit(target));
                    var r   = new Quantity<TemperatureMeasurable>(dto1.Value, m1)
                                  .Add(new Quantity<TemperatureMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }

                var entity = new QuantityMeasurementEntity(dto1, dto2, "ADD", resultValue, resultUnitStr);
                _repository.Save(entity);
                return entity;
            }
            catch (QuantityMeasurementException) { throw; }
            catch (InvalidOperationException ex)
            {
                throw new QuantityMeasurementException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException($"Add failed: {ex.Message}", ex);
            }
        }

        // ── Subtract ──────────────────────────────────────────────────

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
                    var m1  = new LengthMeasurable(ParseLengthUnit(dto1.Unit));
                    var m2  = new LengthMeasurable(ParseLengthUnit(dto2.Unit));
                    var tgt = new LengthMeasurable(ParseLengthUnit(target));
                    var r   = new Quantity<LengthMeasurable>(dto1.Value, m1)
                                  .Subtract(new Quantity<LengthMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "weight")
                {
                    var m1  = new WeightMeasurable(ParseWeightUnit(dto1.Unit));
                    var m2  = new WeightMeasurable(ParseWeightUnit(dto2.Unit));
                    var tgt = new WeightMeasurable(ParseWeightUnit(target));
                    var r   = new Quantity<WeightMeasurable>(dto1.Value, m1)
                                  .Subtract(new Quantity<WeightMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else if (category == "volume")
                {
                    var m1  = new VolumeMeasurable(ParseVolumeUnit(dto1.Unit));
                    var m2  = new VolumeMeasurable(ParseVolumeUnit(dto2.Unit));
                    var tgt = new VolumeMeasurable(ParseVolumeUnit(target));
                    var r   = new Quantity<VolumeMeasurable>(dto1.Value, m1)
                                  .Subtract(new Quantity<VolumeMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }
                else
                {
                    var m1  = new TemperatureMeasurable(ParseTemperatureUnit(dto1.Unit));
                    var m2  = new TemperatureMeasurable(ParseTemperatureUnit(dto2.Unit));
                    var tgt = new TemperatureMeasurable(ParseTemperatureUnit(target));
                    var r   = new Quantity<TemperatureMeasurable>(dto1.Value, m1)
                                  .Subtract(new Quantity<TemperatureMeasurable>(dto2.Value, m2), tgt);
                    resultValue   = r.Value;
                    resultUnitStr = r.Unit.GetUnitName();
                }

                var entity = new QuantityMeasurementEntity(dto1, dto2, "SUBTRACT", resultValue, resultUnitStr);
                _repository.Save(entity);
                return entity;
            }
            catch (QuantityMeasurementException) { throw; }
            catch (InvalidOperationException ex)
            {
                throw new QuantityMeasurementException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException($"Subtract failed: {ex.Message}", ex);
            }
        }

        // ── Divide ────────────────────────────────────────────────────

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
                    var q1 = new Quantity<LengthMeasurable>(dto1.Value, new LengthMeasurable(ParseLengthUnit(dto1.Unit)));
                    var q2 = new Quantity<LengthMeasurable>(dto2.Value, new LengthMeasurable(ParseLengthUnit(dto2.Unit)));
                    scalar = q1.Divide(q2);
                }
                else if (category == "weight")
                {
                    var q1 = new Quantity<WeightMeasurable>(dto1.Value, new WeightMeasurable(ParseWeightUnit(dto1.Unit)));
                    var q2 = new Quantity<WeightMeasurable>(dto2.Value, new WeightMeasurable(ParseWeightUnit(dto2.Unit)));
                    scalar = q1.Divide(q2);
                }
                else if (category == "volume")
                {
                    var q1 = new Quantity<VolumeMeasurable>(dto1.Value, new VolumeMeasurable(ParseVolumeUnit(dto1.Unit)));
                    var q2 = new Quantity<VolumeMeasurable>(dto2.Value, new VolumeMeasurable(ParseVolumeUnit(dto2.Unit)));
                    scalar = q1.Divide(q2);
                }
                else
                {
                    var q1 = new Quantity<TemperatureMeasurable>(dto1.Value, new TemperatureMeasurable(ParseTemperatureUnit(dto1.Unit)));
                    var q2 = new Quantity<TemperatureMeasurable>(dto2.Value, new TemperatureMeasurable(ParseTemperatureUnit(dto2.Unit)));
                    scalar = q1.Divide(q2);
                }

                var entity = new QuantityMeasurementEntity(dto1, dto2, "DIVIDE", scalar, "scalar");
                _repository.Save(entity);
                return entity;
            }
            catch (QuantityMeasurementException) { throw; }
            catch (InvalidOperationException ex)
            {
                throw new QuantityMeasurementException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new QuantityMeasurementException($"Divide failed: {ex.Message}", ex);
            }
        }

        // ── Validation ────────────────────────────────────────────────

        private static void ValidateDTO(QuantityDTO dto, string name)
        {
            if (dto == null)
                throw new QuantityMeasurementException($"{name} cannot be null");
            if (string.IsNullOrWhiteSpace(dto.Unit))
                throw new QuantityMeasurementException($"{name}.Unit cannot be empty");
            if (string.IsNullOrWhiteSpace(dto.Category))
                throw new QuantityMeasurementException($"{name}.Category cannot be empty");
        }

        private static void ValidateSameCategory(QuantityDTO a, QuantityDTO b, string op)
        {
            if (!string.Equals(a.Category, b.Category, StringComparison.OrdinalIgnoreCase))
                throw new QuantityMeasurementException(
                    $"{op}: Cannot mix categories '{a.Category}' and '{b.Category}'");
        }

        // ── Unit parsers ──────────────────────────────────────────────

        private static LengthUnit ParseLengthUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            return u switch
            {
                "FEET"        => LengthUnit.FEET,
                "INCHES"      => LengthUnit.INCHES,
                "YARDS"       => LengthUnit.YARDS,
                "CENTIMETERS" => LengthUnit.CENTIMETERS,
                _ => throw new QuantityMeasurementException($"Invalid LengthUnit: {unit}")
            };
        }

        private static WeightUnit ParseWeightUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            return u switch
            {
                "KILOGRAM" => WeightUnit.KILOGRAM,
                "GRAM"     => WeightUnit.GRAM,
                _ => throw new QuantityMeasurementException($"Invalid WeightUnit: {unit}")
            };
        }

        private static VolumeUnit ParseVolumeUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            return u switch
            {
                "LITRE"      => VolumeUnit.LITRE,
                "MILLILITRE" => VolumeUnit.MILLILITRE,
                "GALLON"     => VolumeUnit.GALLON,
                _ => throw new QuantityMeasurementException($"Invalid VolumeUnit: {unit}")
            };
        }

        private static TemperatureUnit ParseTemperatureUnit(string unit)
        {
            string u = unit.Trim().ToUpper();
            return u switch
            {
                "CELSIUS"    => TemperatureUnit.CELSIUS,
                "FAHRENHEIT" => TemperatureUnit.FAHRENHEIT,
                "KELVIN"     => TemperatureUnit.KELVIN,
                _ => throw new QuantityMeasurementException($"Invalid TemperatureUnit: {unit}")
            };
        }

        // ── Compare dispatcher ────────────────────────────────────────

        private static bool CompareByCategory(QuantityDTO a, QuantityDTO b)
        {
            string category = a.Category.ToLower();

            if (category == "length")
            {
                var q1 = new Quantity<LengthMeasurable>(a.Value, new LengthMeasurable(ParseLengthUnit(a.Unit)));
                var q2 = new Quantity<LengthMeasurable>(b.Value, new LengthMeasurable(ParseLengthUnit(b.Unit)));
                return q1.Equals(q2);
            }
            if (category == "weight")
            {
                var q1 = new Quantity<WeightMeasurable>(a.Value, new WeightMeasurable(ParseWeightUnit(a.Unit)));
                var q2 = new Quantity<WeightMeasurable>(b.Value, new WeightMeasurable(ParseWeightUnit(b.Unit)));
                return q1.Equals(q2);
            }
            if (category == "volume")
            {
                var q1 = new Quantity<VolumeMeasurable>(a.Value, new VolumeMeasurable(ParseVolumeUnit(a.Unit)));
                var q2 = new Quantity<VolumeMeasurable>(b.Value, new VolumeMeasurable(ParseVolumeUnit(b.Unit)));
                return q1.Equals(q2);
            }
            if (category == "temperature")
            {
                var q1 = new Quantity<TemperatureMeasurable>(a.Value, new TemperatureMeasurable(ParseTemperatureUnit(a.Unit)));
                var q2 = new Quantity<TemperatureMeasurable>(b.Value, new TemperatureMeasurable(ParseTemperatureUnit(b.Unit)));
                return q1.Equals(q2);
            }
            throw new QuantityMeasurementException($"Unknown category: {a.Category}");
        }
    }
}
