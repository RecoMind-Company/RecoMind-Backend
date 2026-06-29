using Core.Dtos.Plan;
using FluentValidation;


namespace WebApi.Validators;

internal class UpdateCommentDtoValidator : AbstractValidator<UpdatePlanCommentDto>
{
    public UpdateCommentDtoValidator()
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

