using Core.DTOs.AI;

namespace Core.Exceptions;

public class UnprocessableEntityException : Exception
{
    public ReportValidationError validationError { get; }
    public UnprocessableEntityException(string message, ReportValidationError errors) : base(message)
    {
        validationError = errors;
    }
}
