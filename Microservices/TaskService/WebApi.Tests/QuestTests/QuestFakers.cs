using Bogus;
using Core.Dtos;
using Core.Models;

namespace WebApi.Tests.QuestTests;

public static class QuestFakers
{
    public const string InValid = "InValid";
    public static Faker<QuestDto> CreateQuestDto(int seed = 0) => new Faker<QuestDto>()
        .UseSeed(seed)
        .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
        .RuleFor(q => q.Description, f => seed % 2 == 0 ? f.Lorem.Letter(20) : null)
        .RuleFor(q => q.Status, f => seed % 2 == 0 ? (int)f.PickRandom<QuestStatusEnum>() : null)
        .RuleFor(q => q.StartDate, f => seed % 2 == 0 ? f.Date.Future(1, DateTime.UtcNow.AddSeconds(10)) : null)
        .RuleFor(q => q.DeadLine, (f, q) => seed % 2 == 0 ? q.StartDate!.Value.AddDays(1) : DateTime.UtcNow.AddDays(1))

        .RuleSet(InValid, rules =>
        {
            rules.RuleFor(q => q.Title, f => null);
            rules.RuleFor(q => q.Description, f => f.Lorem.Letter(300));
            rules.RuleFor(q => q.Status, f => 5);
            rules.RuleFor(q => q.StartDate, f => DateTime.UtcNow.AddDays(-5));
            rules.RuleFor(q => q.DeadLine, f => DateTime.UtcNow.AddDays(-6));
        });

    public static Faker<QuestToReturnDto> CreateQuestToRetrunDto(int seed = 0) => new Faker<QuestToReturnDto>()
        .UseSeed(seed)
        .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
        .RuleFor(q => q.Description, f => seed % 2 == 0 ? f.Lorem.Letter(200) : null)
        .RuleFor(q => q.Status, f => seed % 2 == 0 ? f.PickRandom<QuestStatusEnum>() : QuestStatusEnum.active)
        .RuleFor(q => q.StartDate, f => seed % 2 == 0 ? f.Date.Future(1, DateTime.UtcNow.AddSeconds(10)) : DateTime.UtcNow)
        .RuleFor(q => q.DeadLine, (f, q) => seed % 2 == 0 ? q.StartDate.AddDays(1) : DateTime.UtcNow.AddDays(1))
        .RuleFor(q => q.QuestId, f => f.Random.Guid().ToString())
        .RuleFor(q => q.Duration, (f, q) => q.DeadLine - q.StartDate)
        .RuleFor(q => q.PlanId, f => "plan1")
        .RuleFor(q => q.UserAssignedQuests, f => []);
}
