using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QuantityMeasurementRepositoryLayer;

namespace QuantityMeasurementApi.Infrastructure
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await WriteError(context, ex);
            }
        }

        private static async Task WriteError(HttpContext ctx, Exception ex)
        {
            var (status, errorTitle) = ex switch
            {
                QuantityMeasurementException => (HttpStatusCode.BadRequest, "Quantity Measurement Error"),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            var payload = new ApiErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                Status = (int)status,
                Error = errorTitle,
                Message = ex.Message,
                Path = ctx.Request.Path.Value
            };

            ctx.Response.StatusCode = (int)status;
            ctx.Response.ContentType = "application/json";

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}

