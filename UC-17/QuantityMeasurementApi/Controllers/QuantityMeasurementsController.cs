using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementApi.Dtos;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementModelLayer.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace QuantityMeasurementApi.Controllers
{
    [ApiController]
    [Route("api/v1/quantities")]
    [SwaggerTag("REST API for quantity measurement operations")]
    public class QuantityMeasurementsController : ControllerBase
    {
        private readonly IQuantityMeasurementService _service;

        public QuantityMeasurementsController(IQuantityMeasurementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpPost("compare")]
        [SwaggerOperation(Summary = "Compare two quantities")]
        public ActionResult<QuantityMeasurementDto> Compare([FromBody] QuantityInputDto input)
        {
            var e = _service.Compare(ToDomain(input.ThisQuantityDto!), ToDomain(input.ThatQuantityDto!));
            return Ok(QuantityMeasurementDto.FromEntity(e));
        }

        [HttpPost("convert")]
        [SwaggerOperation(Summary = "Convert a quantity to target unit")]
        public ActionResult<QuantityMeasurementDto> Convert([FromBody] QuantityInputDto input)
        {
            // Convention from UC17 doc: use "this" as source and "that" as target unit holder
            var source = ToDomain(input.ThisQuantityDto!);
            var targetUnit = (input.ThatQuantityDto?.Unit ?? "").Trim();
            var e = _service.Convert(source, targetUnit);
            return Ok(QuantityMeasurementDto.FromEntity(e));
        }

        [HttpPost("add")]
        [SwaggerOperation(Summary = "Add two quantities")]
        public ActionResult<QuantityMeasurementDto> Add([FromBody] QuantityInputDto input, [FromQuery] string? targetUnit = null)
        {
            var a = ToDomain(input.ThisQuantityDto!);
            var b = ToDomain(input.ThatQuantityDto!);
            var tgt = string.IsNullOrWhiteSpace(targetUnit) ? (a.Unit ?? "") : targetUnit!;
            var e = _service.Add(a, b, tgt);
            return Ok(QuantityMeasurementDto.FromEntity(e));
        }

        [HttpPost("subtract")]
        [SwaggerOperation(Summary = "Subtract two quantities")]
        public ActionResult<QuantityMeasurementDto> Subtract([FromBody] QuantityInputDto input, [FromQuery] string? targetUnit = null)
        {
            var a = ToDomain(input.ThisQuantityDto!);
            var b = ToDomain(input.ThatQuantityDto!);
            var tgt = string.IsNullOrWhiteSpace(targetUnit) ? (a.Unit ?? "") : targetUnit!;
            var e = _service.Subtract(a, b, tgt);
            return Ok(QuantityMeasurementDto.FromEntity(e));
        }

        [HttpPost("divide")]
        [SwaggerOperation(Summary = "Divide one quantity by another")]
        public ActionResult<QuantityMeasurementDto> Divide([FromBody] QuantityInputDto input)
        {
            var e = _service.Divide(ToDomain(input.ThisQuantityDto!), ToDomain(input.ThatQuantityDto!));
            return Ok(QuantityMeasurementDto.FromEntity(e));
        }

        [HttpGet("history/operation/{operation}")]
        [SwaggerOperation(Summary = "Get operation history by operation type")]
        public ActionResult<List<QuantityMeasurementDto>> HistoryByOperation([FromRoute] string operation)
        {
            var list = _service.GetHistoryByOperation(operation).Select(QuantityMeasurementDto.FromEntity).ToList();
            return Ok(list);
        }

        [HttpGet("history/type/{measurementType}")]
        [SwaggerOperation(Summary = "Get operation history by measurement type")]
        public ActionResult<List<QuantityMeasurementDto>> HistoryByType([FromRoute] string measurementType)
        {
            var list = _service.GetHistoryByType(measurementType).Select(QuantityMeasurementDto.FromEntity).ToList();
            return Ok(list);
        }

        [HttpGet("count/{operation}")]
        [SwaggerOperation(Summary = "Get count for a successful operation type")]
        public ActionResult<int> CountByOperation([FromRoute] string operation)
        {
            return Ok(_service.GetCountByOperation(operation));
        }

        [HttpGet("history/errored")]
        [SwaggerOperation(Summary = "Get errored operation history")]
        public ActionResult<List<QuantityMeasurementDto>> ErroredHistory()
        {
            var list = _service.GetErroredHistory().Select(QuantityMeasurementDto.FromEntity).ToList();
            return Ok(list);
        }

        private static QuantityDTO ToDomain(QuantityDto dto) =>
            new QuantityDTO(dto.Value ?? 0, dto.Unit ?? "", dto.MeasurementType ?? "");
    }
}

