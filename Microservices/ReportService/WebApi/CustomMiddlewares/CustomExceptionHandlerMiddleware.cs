using Core.Exceptions;

namespace WebApi.CustomMiddlewares;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;
    public CustomExceptionHandlerMiddleware(RequestDelegate Next, ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        _next = Next;
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
            _logger.LogError($"Something went wrong: {ex}");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = new ErrorToReturn
        {
            ErrorMessage = ex.Message,
        };
        context.Response.StatusCode = ex switch
        {
            UnprocessableEntityException unprocessableEx => GetValidationErrors(unprocessableEx, response),
            HttpRequestException httpRequestException when httpRequestException.StatusCode.HasValue => (int)httpRequestException.StatusCode.Value,
            _ => StatusCodes.Status500InternalServerError
        };
        response.StatusCode = context.Response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);

    }
    private static int GetValidationErrors(UnprocessableEntityException ex, ErrorToReturn response)
    {
        response.Errors = ex.validationError.Details;
        return StatusCodes.Status422UnprocessableEntity;
    }
}
