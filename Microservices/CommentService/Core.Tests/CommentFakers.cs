using Bogus;
using Core.Dtos;
using Core.Dtos.Plan;
using Core.Models;

namespace Core.Tests;

internal static class CommentFakers
{
    internal static Faker<Comment> GetComment(int seed = 0) => new Faker<Comment>()
        .UseSeed(seed)
        .RuleFor(c => c.Id, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.PlanId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedAt, f => seed % 2 == 0 ? DateTime.UtcNow : f.Date.Past());

    internal static Faker<CommentDto> GetCommentDto(int seed = 0) => new Faker<CommentDto>()
        .UseSeed(seed)
        .RuleFor(c => c.Id, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedAt, f => seed % 2 == 0 ? DateTime.UtcNow : f.Date.Past());

    internal static Faker<AddCommentDto> GetAddCommentDto(int seed = 0) => new Faker<AddCommentDto>()
        .UseSeed(seed)
        .RuleFor(c => c.UserId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.PlanId, f => f.Random.Guid().ToString())
        .RuleFor(c => c.UserComment, f => f.Lorem.Sentence());

    internal static Faker<UpdateCommentDto> GetUpdateCommentDto(int seed = 0) => new Faker<UpdateCommentDto>()
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
