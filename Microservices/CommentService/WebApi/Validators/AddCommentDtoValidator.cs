using Core.Dtos.Plan;
using FluentValidation;

namespace WebApi.Validators;

internal class AddCommentDtoValidator : AbstractValidator<AddPlanCommentDto>
{
    public AddCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .NotEmpty().WithMessage("User comment is required.")
            .MaximumLength(500);

    }
}
