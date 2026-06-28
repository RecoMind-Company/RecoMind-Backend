using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

public class QuestByStatusDtoValidator : AbstractValidator<QuestByStatusDto>
{
    public QuestByStatusDtoValidator()
    {
        RuleFor(q => q.Status)
             .InclusiveBetween(0, 4)
             .When(q => q.Status is not null)
             .WithMessage("Status must be between 0 and 4. 0: to_do 1: in_progress 2: completed 3: blocked 4: overdue");
    }
}
