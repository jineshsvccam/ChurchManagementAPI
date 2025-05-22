using System.Net;
using System.Text.Json;

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
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = "Invalid request parameters.";
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = "Invalid operation.";
                    break;

                case FormatException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorMessage = "Invalid format.";
                    break;

                case TimeoutException:
                    statusCode = HttpStatusCode.RequestTimeout;
                    errorMessage = "Request timed out.";
                    break;

                    // Default case: Keep it as InternalServerError
            }
//
            // Log full exception details including inner exceptions
            _logger.LogError(exception, "Exception Occurred: {ExceptionType} | Path: {RequestPath} | Message: {ErrorMessage}",
                exception.GetType().Name, context.Request.Path, exception.Message);

            LogInnerExceptions(exception);

            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = errorMessage,
                Detailed = _env.IsDevelopment() ? exception.Message : null,
                StackTrace = _env.IsDevelopment() && exception.InnerException != null ? exception.InnerException.ToString() : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }

       
        private void LogInnerExceptions(Exception ex)
        {
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                _logger.LogError(inner, "Inner Exception: {ExceptionType} | Message: {ErrorMessage}", inner.GetType().Name, inner.Message);
                inner = inner.InnerException;
            }
        }
    }
}
