using Core.Result;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected async Task<Result<bool>> ExecuteValidation<T>(IValidator<T> validator, T dto)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(x => new Error(x.PropertyName, x.ErrorMessage));
            return Result<bool>.Failure(errors);
        }
        return Result<bool>.Success(true);
    }
    protected ActionResult HandleFailure(IEnumerable<Error> errors)
    {
        var firstError = errors.First();
        var errorCode = firstError.code ?? string.Empty;

        var actionResult = errorCode switch
        {
            _ when errorCode.Contains("NotFound") => NotFound(errors),
            _ when errorCode.Contains("AlreadyExist") => Conflict(errors),
            _ when errorCode.Contains("AccessDenied") => StatusCode(StatusCodes.Status403Forbidden, errors),
            _ when errorCode.Contains("Unauthorized") => Unauthorized(errors),
            _ => BadRequest(errors)
        };
        return actionResult;
    }
}
