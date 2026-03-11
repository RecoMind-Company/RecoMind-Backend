using Bogus;
using Core.Models;

namespace WebApi.Tests.UserQuestsTests;

internal class UserQuestFaker
{
    internal static Faker<UserQuests> UserQuests(int seed = 0) => new Faker<UserQuests>()
        .Rules((f, uq) =>
        {
            uq.QuestId = f.Random.Guid().ToString();
            uq.UserId = f.Random.Guid().ToString();
            uq.Quest = null;
        });
}
