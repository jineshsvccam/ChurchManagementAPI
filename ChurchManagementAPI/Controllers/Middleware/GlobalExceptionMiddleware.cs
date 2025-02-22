using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace ChurchManagementAPI.Controllers.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // Default 500
            string errorMessage = "An unexpected error occurred. Please try again later.";

            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorMessage = "Resource not found.";
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorMessage = "Unauthorized access.";
                    break;

                case ArgumentNullException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = "Invalid request parameters.";
                    break;

                    // Add more custom exception handlers if needed.
            }

            _logger.LogError(exception, "Exception: {ExceptionType} | Path: {RequestPath} | Message: {ErrorMessage}",
                exception.GetType().Name, context.Request.Path, exception.Message);

            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = errorMessage,
                Detailed = _env.IsDevelopment() ? exception.Message : null // Show detailed message only in development
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
