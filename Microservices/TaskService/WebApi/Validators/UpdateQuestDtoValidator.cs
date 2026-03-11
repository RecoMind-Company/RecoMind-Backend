using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

internal class UpdateQuestDtoValidator : AbstractValidator<UpdateQuestDto>
{
    public UpdateQuestDtoValidator()
    {
        RuleFor(q => q.Title)
            .MaximumLength(100)
            .WithMessage("Title must not exceed 100 characters.")
            .When(q => !string.IsNullOrEmpty(q.Title));

        RuleFor(q => q.Description)
            .MaximumLength(255)
            .When(q => !string.IsNullOrEmpty(q.Description));

        RuleFor(q => q.Status)
            .InclusiveBetween(0, 3)
            .WithMessage("Status must be between 0 and 3. 0: pending 1: active 2: completed 3: action_required")
            .When(q => q.Status is not null);

        RuleFor(q => q.StartDate)
            .Must(date => date >= DateTime.UtcNow.AddMinutes(-2)).WithMessage("Start date must be in the future.")
            .When(q => q.StartDate is not null);

        RuleFor(q => q.DeadLine)
            .Must((q, deadline) => deadline >= (q.StartDate ?? DateTime.UtcNow.AddMinutes(-1)))
            .WithMessage("Deadline must be in the future and after the start date.")
            .When(q => q.StartDate.HasValue && q.DeadLine.HasValue);
    }
}
