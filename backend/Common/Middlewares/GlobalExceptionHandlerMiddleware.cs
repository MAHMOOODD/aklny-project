using Resturant_Backend.Common.Exceptions;
using Resturant_Backend.Common.Responses;
using System.Net;
using System.Text.Json;

namespace Resturant_Backend.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing request: {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            string message;
            IDictionary<string, List<string>>? errors = null;

            switch(exception)
            {
                case ValidationException validationEx:
                    statusCode = validationEx.StatusCode;
                    message = validationEx.Message;
                    errors = validationEx.ValidationErrors;
                    break;

                case AppException appEx:
                    statusCode = appEx.StatusCode;
                    message = appEx.Message;
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = _environment.IsDevelopment()
                        ? $"Internal Server Error: {exception.Message}"
                        : "حدث خطأ غير متوقع في السيرفر. يرجى المحاولة مرة أخرى.";

                    _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                    break;
            }

            await WriteCustomResponseAsync(context, statusCode, message, errors);
        }

        private static async Task WriteCustomResponseAsync(
            HttpContext context,
            int statusCode,
            string message,
            IDictionary<string, List<string>>? errors = null)
        {
            if(context.Response.HasStarted)
            {
                return;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = ApiResponse<object>.FailureResponse(statusCode, message, errors);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}