using Bogus;
using Core.Dtos;
using Core.Models;
using Core.Tests.User.Quests;

namespace Core.Tests.Quests;

public static class FakeQuests
{
    public static Faker<Quest> GetFakeQuest(int seed = 0) => new Faker<Quest>()
        .UseSeed(seed)
        .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
        .RuleFor(q => q.Description, f => f.Lorem.Paragraph())
        .RuleFor(q => q.StartDate, f => f.Date.Past())
        .RuleFor(q => q.DeadLine, (f, q) => q.StartDate.AddDays(f.Random.Int(1, 30)))
        .RuleFor(q => q.Status, f => f.PickRandom<QuestStatusEnum>())
        .RuleFor(q => q.QuestId, f => f.Random.Guid().ToString())
        .RuleFor(q => q.PlanId, f => "plan1")
        .RuleFor(q => q.UserAssignedQuests, f => FakeUserQuests.GetFakeUserQuests(seed).Generate(3));
    public static Faker<QuestDto> GetFakeQuestDto(int seed = 0) => new Faker<QuestDto>()
            .UseSeed(seed)
            .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
            .RuleFor(q => q.Description, f => f.Lorem.Paragraph())
            .RuleFor(q => q.StartDate, f => f.Date.Past())
            .RuleFor(q => q.DeadLine, (f, q) => q.StartDate?.AddDays(f.Random.Int(1, 30)))
            .RuleFor(q => q.Status, f => f.Random.Bool(0.2f) ? null : (int)f.PickRandom<QuestStatusEnum>());
    public static Faker<QuestToReturnDto> GetFakeQuestToReturnDto(int seed = 0) => new Faker<QuestToReturnDto>()
            .UseSeed(seed)
            .RuleFor(q => q.Title, f => f.Lorem.Sentence(3))
            .RuleFor(q => q.Description, f => f.Lorem.Paragraph())
            .RuleFor(q => q.StartDate, f => f.Date.Past())
            .RuleFor(q => q.DeadLine, (f, q) => q.StartDate.AddDays(f.Random.Int(1, 30)))
            .RuleFor(q => q.Status, f => f.PickRandom<QuestStatusEnum>())
            .RuleFor(q => q.QuestId, f => f.Random.Guid().ToString())
            .RuleFor(q => q.Duration, (f, q) => q.DeadLine - q.StartDate)
            .RuleFor(q => q.PlanId, f => "plan1")
            .RuleFor(q => q.UserAssignedQuests, f => f.Make(f.Random.Int(1, 5), () => f.Random.Guid().ToString()).ToList());
    public static Faker<UpdateQuestDto> GetFakeUpdateQuestDto(int seed = 0) => new Faker<UpdateQuestDto>()
        .UseSeed(seed)
        .RuleFor(q => q.Title, f => f.Random.Bool(0.5f) ? null : f.Lorem.Sentence(3))
        .RuleFor(q => q.Description, f => f.Random.Bool(0.5f) ? null : f.Lorem.Paragraph())
        .RuleFor(q => q.StartDate, f => f.Random.Bool(0.5f) ? null : f.Date.Past())
        .RuleFor(q => q.DeadLine, (f, q) => q.StartDate != null ? (DateTime?)q.StartDate.Value.AddDays(f.Random.Int(1, 30)) : null)
        .RuleFor(q => q.Status, f => f.Random.Bool(0.5f) ? null : (int?)f.PickRandom<QuestStatusEnum>());
    public static Faker<QuestByStatusDto> GetFakeQuestByStatusDto(int seed = 0) => new Faker<QuestByStatusDto>()
        .UseSeed(seed)
        .RuleFor(q => q.Status, f => (int)f.PickRandom<QuestStatusEnum>());
}