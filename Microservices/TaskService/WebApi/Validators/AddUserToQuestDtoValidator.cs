using Core.Dtos;
using FluentValidation;

namespace WebApi.Validators;

public class AddUserToQuestDtoValidator : AbstractValidator<AddUserToQuestDto>
{
    public AddUserToQuestDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.QuestId).NotEmpty().WithMessage("QuestId is required.");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("TeamId is required.");
    }
}
