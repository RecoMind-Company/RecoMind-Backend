using Core.Result;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected async Task<Result<bool>> ExecuteValidation<T>(T dto, IValidator<T> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new Error(e.PropertyName, e.ErrorMessage)).ToList();
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
            _ => BadRequest(errors)
        };
        return actionResult;
    }
}
