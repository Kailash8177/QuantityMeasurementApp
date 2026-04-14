using System.Net;
using System.Text.Json;
using QuantityMeasurementRepositoryLayer;

namespace QuantityMeasurementApi.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
                await WriteError(context, ex);
            }
        }

        private static async Task WriteError(HttpContext ctx, Exception ex)
        {
            var (status, errorTitle) = ex switch
            {
                QuantityMeasurementException => (HttpStatusCode.BadRequest, "Quantity Measurement Error"),
                ArgumentException            => (HttpStatusCode.BadRequest, "Bad Request"),
                _                            => (HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            var payload = new
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                Status    = (int)status,
                Error     = errorTitle,
                Message   = ex.Message,
                Path      = ctx.Request.Path.Value
            };

            ctx.Response.StatusCode  = (int)status;
            ctx.Response.ContentType = "application/json";

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
