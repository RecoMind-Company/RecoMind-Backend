using Core.Dtos.Plan;
using FluentValidation;

namespace WebApi.Validators.PlanComments;

internal class AddiPlanCommentDtoValidator : AbstractValidator<AddPlanCommentDto>
{
    public AddiPlanCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .NotEmpty().WithMessage("User comment is required.")
            .MaximumLength(500);

    }
}
