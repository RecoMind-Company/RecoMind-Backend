using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators
{
    public class QuestDtoValidator : AbstractValidator<QuestDto>
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
                .When(q => q.Status is not null);

            RuleFor(q => q.StartDate)
                .Must(date => date >= DateTime.UtcNow.AddMinutes(-2)).WithMessage("Start date must be in the future.")
                .When(q => q.StartDate is not null);

            RuleFor(q => q.DeadLine)
                .NotEmpty().WithMessage("Deadline is required.")
                .Must(date => date >= DateTime.UtcNow.AddMinutes(-2)).WithMessage("Deadline must be in the future.");
        }
    }
}
