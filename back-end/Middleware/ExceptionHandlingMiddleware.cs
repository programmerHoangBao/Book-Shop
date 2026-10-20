using back_end.DTOs;
using back_end.Exceptions;
using back_end.Records;
using System.Text.Json;

namespace back_end.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware
        (
            RequestDelegate next, 
            ILogger<ExceptionHandlingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        private static async Task HandleExceptionAsync
        (
            HttpContext context, 
            Exception exception
        )
        {
            string? message = null;
            ErrorRecord errorRecord = exception switch
            {
                BusinessException businessException =>
                    businessException.ErrorRecord,
                ArgumentException =>
                    ErrorRecord.RequestInvalid,
                KeyNotFoundException =>
                    ErrorRecord.RequestNotFound,
                _ =>
                    ErrorRecord.InternalServerError
            };
            if (exception is ArgumentException || exception is KeyNotFoundException)
            {
                message = exception.Message;
            }
            var response = ApiResponse<object?>.Response(errorRecord: errorRecord, data:message);
            context.Response.StatusCode = errorRecord.HttpStatus;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Unhandled exception occurred. Request: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path
                );
                await HandleExceptionAsync(context, ex);
            }
        }
    }
}
