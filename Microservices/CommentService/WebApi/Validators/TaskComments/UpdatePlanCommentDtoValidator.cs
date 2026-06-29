using Core.Dtos.Task;
using FluentValidation;


namespace WebApi.Validators.TaskComments;

internal class UpdateTaskCommentDtoValidator : AbstractValidator<UpdateTaskCommentDto>
{
    public UpdateTaskCommentDtoValidator()
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

