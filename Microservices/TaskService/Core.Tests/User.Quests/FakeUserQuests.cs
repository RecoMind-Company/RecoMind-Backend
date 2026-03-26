using Bogus;
using Core.Dtos;
using Core.Models;

namespace Core.Tests.User.Quests;

public static class FakeUserQuests
{
    public const string ValidRuleSet = "valid";
    public const string UserAssignedRuleSet = "userAssigned";
    public static Faker<UserQuests> GetFakeUserQuests(int seed = 0) => new Faker<UserQuests>()
        .UseSeed(seed)
        .Rules((f, uq) =>
        {
            uq.QuestId = f.Random.Guid().ToString();
            uq.UserId = f.Random.Guid().ToString();
            uq.Quest = null;
        });
    public static Faker<AddUserToQuestDto> GetFakeAddUserToQuestDto(int seed = 0) => new Faker<AddUserToQuestDto>()
        .UseSeed(seed)
        .RuleSet(ValidRuleSet, r =>
            r.Rules((f, uq) =>
            {
                uq.TeamId = f.Random.Guid().ToString();
                uq.QuestId = f.Random.Guid().ToString();
                uq.UserId = f.Random.Guid().ToString();
            })
        )
        .RuleSet(UserAssignedRuleSet, r =>
            r.Rules((f, uq) =>
            {
                uq.QuestId = f.Random.Guid().ToString();
                uq.UserId = f.Random.Guid().ToString();
                uq.TeamId = f.Random.Guid().ToString();
            })
        );


}
