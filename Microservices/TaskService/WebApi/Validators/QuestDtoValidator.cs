using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

internal class QuestDtoValidator : AbstractValidator<QuestDto>
{
    public QuestDtoValidator()
    {
        RuleFor(q => q.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(100)
            .WithMessage("Title must not exceed 100 characters.");

        RuleFor(q => q.Description)
            .MaximumLength(255)
            .When(q => !string.IsNullOrEmpty(q.Description));

        RuleFor(q => q.Status)
            .InclusiveBetween(0, 3)
            .When(q => q.Status is not null)
            .WithMessage("Status must be between 0 and 3. 0: pending 1: active 2: completed 3: action_required");

        RuleFor(q => q.StartDate)
            .Must(startDate => startDate >= DateTime.UtcNow.AddMinutes(-2)).WithMessage("Start date must be in the future.")
            .When(q => q.StartDate is not null);

        RuleFor(q => q.DeadLine)
            .NotEmpty().WithMessage("Deadline is required.")
            .Must((dto, deadline) => deadline >= (dto.StartDate ?? DateTime.UtcNow.AddMinutes(-1)))
            .WithMessage("Deadline must be in the future and after the start date.");
    }
}
