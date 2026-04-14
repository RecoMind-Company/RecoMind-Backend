using AutoMapper;
using Core.Dtos;
using Core.Interface;
using Core.Models;
using Core.Result;
using Core.ServicesAbstraction;

namespace Core.Services;

public class CommentService(IUnitOfWork unitOfWork,
                            IMapper mapper,
                            IGrpcTeamService grpcTeamService,
                            IGrpcPlanService grpcPlanService) : ICommentService
{
    private readonly IGenericRepository<Comment> _commentRepository = unitOfWork.GetRepository<Comment>();
    public async Task<Result<CommentDto>> AddCommentAsync(AddCommentDto addCommentDto)
    {
        // TODO: the main two validation
        // 1- check if plan exists 
        // 2- check if user is in the provided team by (userId) and (teamId) which is related to the plan
        var plan = await grpcPlanService.GetPlanIdsAsync(addCommentDto.PlanId!);
        if (!plan.IsExisted)
            return Result<CommentDto>.Failure(PlanErrors.PlanNotFound);

        if (
            await grpcPlanService.IsOwnerOfPlanAsync(addCommentDto.UserId!, addCommentDto.PlanId!)
            ||
            await grpcTeamService.IsUserExist(addCommentDto.UserId!, plan.TeamId!))
        {
            var comment = mapper.Map<Comment>(addCommentDto);
            await _commentRepository.AddAsync(comment);
            await unitOfWork.SaveChangesAsync();
            var commentDto = mapper.Map<CommentDto>(comment);
            return Result<CommentDto>.Success(commentDto);
        }
        return Result<CommentDto>.Failure(CommentErrors.AccessDenied);
    }
    public async Task<Result<IEnumerable<CommentDto>>> GetCommentsByPlanIdAsync(string planId)
    {
        var isPlanExist = await grpcPlanService.GetPlanIdsAsync(planId);
        if (!isPlanExist.IsExisted)
            return Result<IEnumerable<CommentDto>>.Failure(PlanErrors.PlanNotFound);

        var comments = await _commentRepository.FindAll(c =>
            c.PlanId == planId,
            orderBy: x => x.OrderByDescending(y => y.CreatedAt)
        );
        var commentDtos = mapper.Map<IEnumerable<CommentDto>>(comments);
        return Result<IEnumerable<CommentDto>>.Success(commentDtos);
    }
    public async Task<Result<CommentDto>> UpdateCommentAsync(UpdateCommentDto updateCommentDto)
    {
        var comment = await _commentRepository.Find(c => c.Id == updateCommentDto.CommentId);

        if (comment == null)
            return Result<CommentDto>.Failure(CommentErrors.NotFound);
        if (comment.UserId != updateCommentDto.UserId)
            return Result<CommentDto>.Failure(CommentErrors.AccessDenied);
        if (DateTime.UtcNow > comment.CreatedAt.AddMinutes(5))
            return Result<CommentDto>.Failure(CommentErrors.EditTimeout);
        mapper.Map(updateCommentDto, comment);
        _commentRepository.Update(comment);
        await unitOfWork.SaveChangesAsync();

        var commentDto = mapper.Map<CommentDto>(comment);
        return Result<CommentDto>.Success(commentDto);
    }
    public async Task<Result<bool>> DeleteCommentAsync(string commentId, string userId)
    {
        var comment = await _commentRepository.Find(c => c.Id == commentId);

        if (comment == null)
            return Result<bool>.Failure(CommentErrors.NotFound);
        if (comment.UserId != userId)
            return Result<bool>.Failure(CommentErrors.AccessDenied);

        _commentRepository.Delete(comment);
        await unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
