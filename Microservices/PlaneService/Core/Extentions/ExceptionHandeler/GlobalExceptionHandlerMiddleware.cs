using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extentions.ExceptionHandeler
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }


        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            string title = "Internal Server Error";

            string detail = "An unexpected error occurred. Please try again later.";

            if (exception is KeyNotFoundException)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                title = "Not Found";
                detail = "The requested resource could not be found.";
            }
            else if (exception is ArgumentException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                title = "Bad Request";
                detail = "The request contains invalid parameters.";
            }

            var problemDetails = new
            {
                status = context.Response.StatusCode,
                title = title,
                detail = detail,
            };

            return context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
