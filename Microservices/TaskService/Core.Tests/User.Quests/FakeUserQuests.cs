using Bogus;
using Core.Models;

namespace Core.Tests.User.Quests;

public static class FakeUserQuests
{
    public static Faker<UserQuests> GetFakeUserQuests(int seed = 0) => new Faker<UserQuests>()
        .UseSeed(seed)
        .RuleSet("default", rules =>
        {
            rules.RuleFor(uq => uq.QuestId, f => f.Random.Guid().ToString());
            rules.RuleFor(uq => uq.UserId, f => f.Random.Guid().ToString());
        });
}
