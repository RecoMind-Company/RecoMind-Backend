using Core.Dtos.Plan;
using FluentValidation;


namespace WebApi.Validators.PlanComments;

internal class UpdatePlanCommentDtoValidator : AbstractValidator<UpdatePlanCommentDto>
{
    public UpdatePlanCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .MaximumLength(500)
            .WithMessage("User comment must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.UserComment));

        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required.");
    }
}

