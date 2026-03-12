using Bogus;
using Core.Dtos;
using Core.Models;

namespace WebApi.Tests.UserQuestsTests;

internal class UserQuestFakers
{
    internal const string InValid = "InValid";
    internal static Faker<UserQuests> UserQuests(int seed = 0) => new Faker<UserQuests>()
        .UseSeed(seed)
        .Rules((f, uq) =>
        {
            uq.QuestId = f.Random.Guid().ToString();
            uq.UserId = f.Random.Guid().ToString();
            uq.Quest = null;
        });
    internal static Faker<AddUserToQuestDto> AddUserToQuest(int seed = 0) => new Faker<AddUserToQuestDto>()
        .UseSeed(seed)
        .Rules((f, auq) =>
        {
            auq.QuestId = f.Random.Guid().ToString();
            auq.UserId = f.Random.Guid().ToString();
            auq.TeamId = f.Random.Guid().ToString();
        })
        .RuleSet(InValid, ruleset =>
        {
            ruleset.RuleFor(auq => auq.QuestId, f => null);
            ruleset.RuleFor(auq => auq.UserId, f => null);
            ruleset.RuleFor(auq => auq.TeamId, f => null);
        });
}
