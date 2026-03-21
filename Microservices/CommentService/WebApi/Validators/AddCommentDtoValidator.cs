using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

internal class AddCommentDtoValidator : AbstractValidator<AddCommentDto>
{
    public AddCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("User comment is required.");
    }
}
