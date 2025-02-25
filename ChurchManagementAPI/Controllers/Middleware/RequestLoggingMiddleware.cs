using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private const int MaxResponseBodyLength = 1000; // Limit response body size in logs

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var requestPath = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
            var requestBody = await ReadRequestBodyAsync(context);

            _logger.LogInformation("Incoming Request: {RequestPath} | User: {UserId} | IP: {IPAddress} | Body: {RequestBody}",
                requestPath, userId, ipAddress, requestBody);

            var originalResponseBodyStream = context.Response.Body;
            await using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);

              //  if (context.Request.Method != "GET") // Avoid logging large GET responses
                {
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    var responseBody = await new StreamReader(responseBodyStream, Encoding.UTF8).ReadToEndAsync();
                    responseBodyStream.Seek(0, SeekOrigin.Begin);

                    if (responseBody.Length > MaxResponseBodyLength)
                    {
                        responseBody = responseBody.Substring(0, MaxResponseBodyLength) + "... [Truncated]";
                    }

                    _logger.LogInformation("Response: {StatusCode} for {RequestPath} | Time Taken: {ElapsedMs} ms | Response Body: {ResponseBody}",
                        context.Response.StatusCode, requestPath, stopwatch.ElapsedMilliseconds, responseBody);
                }

                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            finally
            {
                context.Response.Body = originalResponseBodyStream;
                stopwatch.Stop();
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
                return "<No Body>";

            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            return body.Length > MaxResponseBodyLength ? body.Substring(0, MaxResponseBodyLength) + "... [Truncated]" : body;
        }
    }
}
