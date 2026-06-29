using Bogus;
using Core.Dtos.Plan;
using Core.Models;

namespace Core.Tests;

internal static class CommentFakers
{
    internal static Faker<PlanComment> GetComment(int seed = 0) => new Faker<PlanComment>()
        .UseSeed(seed)
        .RuleFor(c => c.Id, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.PlanId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedAt, f => seed % 2 == 0 ? DateTime.UtcNow : f.Date.Past());

    internal static Faker<PlanCommentDto> GetCommentDto(int seed = 0) => new Faker<PlanCommentDto>()
        .UseSeed(seed)
        .RuleFor(c => c.Id, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedAt, f => seed % 2 == 0 ? DateTime.UtcNow : f.Date.Past());

    internal static Faker<AddPlanCommentDto> GetAddCommentDto(int seed = 0) => new Faker<AddPlanCommentDto>()
        .UseSeed(seed)
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.PlanId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence());

    internal static Faker<UpdatePlanCommentDto> GetUpdateCommentDto(int seed = 0) => new Faker<UpdatePlanCommentDto>()
        .UseSeed(seed)
        .RuleFor(c => c.CommentId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence());

    internal static Faker<PlanIdsDto> GetPlanIdsDto(int seed = 0) => new Faker<PlanIdsDto>()
        .UseSeed(seed)
        .RuleFor(p => p.OwnerId, f => f.Random.Guid().ToString())
        .RuleFor(p => p.PlanId, f => f.Random.Guid().ToString())
        .RuleFor(p => p.TeamId, f => f.Random.Guid().ToString())
        .RuleFor(p => p.CompanyId, f => f.Random.Guid().ToString())
        .RuleFor(p => p.IsExisted, f => seed % 2 == 0 ? true : false);

}
