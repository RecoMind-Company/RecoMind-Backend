using Core.Dtos.Task;
using FluentValidation;

namespace WebApi.Validators.TaskComments;

internal class AddTaskCommentDtoValidator : AbstractValidator<AddTaskCommentDto>
{
    public AddTaskCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .NotEmpty().WithMessage("User comment is required.")
            .MaximumLength(500);

    }
}
