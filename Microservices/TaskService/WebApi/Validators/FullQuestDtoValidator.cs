using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators
{
    public class FullQuestDtoValidator : AbstractValidator<FullQuestDto>
    {
        public FullQuestDtoValidator()
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
                 .InclusiveBetween(0, 4)
                 .When(q => q.Status is not null)
                 .WithMessage("Status must be between 0 and 4. 0: to_do 1: in_progress 2: completed 3: blocked 4: overdue");

            RuleFor(q => q.Priority)
                .InclusiveBetween(0, 2)
                .When(q => q.Priority is not null)
                .WithMessage("Priority must be between 0 and 3. 0: low 1: medium 2: high");

            RuleFor(q => q.StartDate)
                .Must(startDate => startDate >= DateTime.UtcNow.AddMinutes(-2)).WithMessage("Start date must be in the future.")
                .When(q => q.StartDate is not null);

            RuleFor(q => q.DeadLine)
                .NotEmpty().WithMessage("Deadline is required.")
                .Must((dto, deadline) => deadline >= (dto.StartDate ?? DateTime.UtcNow.AddMinutes(-1)))
                .WithMessage("Deadline must be in the future and after the start date.");

        }
    }
}
