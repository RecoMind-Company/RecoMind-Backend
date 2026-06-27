using Bogus;
using Core.Dtos;
using Core.Models;

namespace WebApi.Tests.QuestTests;

internal static class QuestFakers
{
    internal const string InValid = "InValid";
    internal static Faker<QuestDto> QuestDto(int seed = 0) => new Faker<QuestDto>()
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

    internal static Faker<QuestToReturnDto> QuestToReturnDto(int seed = 0) => new Faker<QuestToReturnDto>()
        .UseSeed(seed)
        .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
        .RuleFor(q => q.Description, f => seed % 2 == 0 ? f.Lorem.Letter(20) : null)
        .RuleFor(q => q.Status, f => seed % 2 == 0 ? f.PickRandom<QuestStatusEnum>() : QuestStatusEnum.in_progress)
        .RuleFor(q => q.StartDate, f => seed % 2 == 0 ? f.Date.Future(1, DateTime.UtcNow.AddSeconds(10)) : DateTime.UtcNow)
        .RuleFor(q => q.DeadLine, (f, q) => seed % 2 == 0 ? q.StartDate.AddDays(1) : DateTime.UtcNow.AddDays(1))
        .RuleFor(q => q.QuestId, f => f.Random.Guid().ToString())
        .RuleFor(q => q.Duration, (f, q) => q.DeadLine - q.StartDate)
        .RuleFor(q => q.ModuleId, f => seed % 2 == 0 ? "module1" : "module2")
        .RuleFor(q => q.UserAssignedQuests, f => []);
    internal static Faker<Quest> Quest(int seed = 0) => new Faker<Quest>()
         .UseSeed(seed)
        .Rules((f, q) =>
        {
            q.Title = f.Lorem.Sentence(3);
            q.Description = seed % 2 == 0 ? f.Lorem.Letter(20) : null;
            q.Status = f.PickRandom<QuestStatusEnum>();
            q.StartDate = f.Date.Future(1, DateTime.UtcNow.AddSeconds(10));
            q.DeadLine = q.StartDate.AddDays(1);
            q.ModuleId = "module1";
            q.QuestId = f.Random.Guid().ToString();
            q.UserAssignedQuests = [];
        });
    internal static Faker<UpdateQuestDto> UpdateQuestDto(int seed = 0) => new Faker<UpdateQuestDto>()
        .UseSeed(seed)
        .Rules((f, uq) =>
        {
            uq.Title = f.Random.Bool(0.5f) ? f.Lorem.Sentence(3) : null;
            uq.Description = f.Random.Bool(0.5f) ? f.Lorem.Letter(20) : null;
            uq.Status = f.Random.Bool(0.5f) ? (int?)f.PickRandom<QuestStatusEnum>() : null;
            uq.StartDate = seed % 2 == 0 ? null : f.Date.Future(1, DateTime.UtcNow.AddSeconds(10));
            uq.DeadLine = seed % 2 == 0 ? null : uq.StartDate!.Value.AddDays(1);
        })
        .RuleSet(InValid, rules =>
        {
            rules.RuleFor(q => q.Title, f => f.Lorem.Letter(300));
            rules.RuleFor(q => q.Description, f => f.Lorem.Letter(300));
            rules.RuleFor(q => q.Status, f => 5);
            rules.RuleFor(q => q.StartDate, f => DateTime.UtcNow.AddDays(-5));
            rules.RuleFor(q => q.DeadLine, f => DateTime.UtcNow.AddDays(-6));
        });
}
