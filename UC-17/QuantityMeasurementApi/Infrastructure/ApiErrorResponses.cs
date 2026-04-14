using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace QuantityMeasurementApi.Infrastructure
{
    public static class ApiErrorResponses
    {
        public static BadRequestObjectResult ValidationProblem(ActionContext ctx)
        {
            var errors = ctx.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage).ToArray()
                );

            var first = errors.SelectMany(kvp => kvp.Value.Select(msg => (path: kvp.Key, msg))).FirstOrDefault();

            return new BadRequestObjectResult(new ApiErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                Status = 400,
                Error = "Quantity Measurement Error",
                Message = string.IsNullOrWhiteSpace(first.msg) ? "Validation failed" : first.msg,
                Path = string.IsNullOrWhiteSpace(first.path) ? ctx.HttpContext.Request.Path.Value : first.path,
                ValidationErrors = errors.Count == 0 ? null : errors
            });
        }
    }

    public class ApiErrorResponse
    {
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Path { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}

