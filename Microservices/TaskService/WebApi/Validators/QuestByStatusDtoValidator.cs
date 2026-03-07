using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

public class QuestByStatusDtoValidator : AbstractValidator<QuestByStatusDto>
{
    public QuestByStatusDtoValidator()
    {
        RuleFor(q => q.Status)
        .InclusiveBetween(0, 3)
        .When(q => q.Status is not null);
    }
}
