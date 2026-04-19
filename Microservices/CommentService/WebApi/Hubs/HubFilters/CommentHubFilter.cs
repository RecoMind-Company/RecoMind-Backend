using FluentValidation;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs.HubFilters;

public class CommentHubFilter(ILogger<CommentHubFilter> logger) : IHubFilter
{
    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {
        logger.LogInformation($"Calling hub method '{invocationContext.HubMethodName}'");
        try
        {
            var arguments = invocationContext.HubMethodArguments.ToList();
            foreach (var arg in arguments)
            {
                if (arg is null) continue;
                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                var validator = invocationContext.ServiceProvider.GetService(validatorType) as IValidator;

                if (validator is not null)
                {
                    var context = new ValidationContext<object>(arg);
                    var result = await validator.ValidateAsync(context);
                    if (!result.IsValid)
                    {
                        await invocationContext.Hub.Clients.Caller.SendAsync("ReceiveValidationErrors", result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
                        throw new ValidationException(result.Errors);
                    }
                }
            }
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception calling '{invocationContext.HubMethodName}': {ex}");
            throw;
        }
    }
}
