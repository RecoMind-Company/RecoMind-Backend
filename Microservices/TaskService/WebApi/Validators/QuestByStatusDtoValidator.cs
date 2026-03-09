using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

public class QuestByStatusDtoValidator : AbstractValidator<QuestByStatusDto>
{
    public QuestByStatusDtoValidator()
    {
        RuleFor(q => q.Status)
             .InclusiveBetween(0, 3)
             .When(q => q.Status is not null)
             .WithMessage("Status must be between 0 and 3. 0: pending 1: active 2: completed 3: action_required");
    }
}
