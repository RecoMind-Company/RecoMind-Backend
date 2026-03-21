using Core.Dtos;
using FluentValidation;


namespace WebApi.Validators;

internal class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
{
    public UpdateCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .MaximumLength(500)
            .WithMessage("User comment must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.UserComment));
    }
}

