using AutoMapper;
using Core.Dtos.Plan;
using Core.Interface;
using Core.Models;
using Core.Result;
using Core.ServicesAbstraction;
using Core.ServicesAbstractions;
using RecoMind.Contracts.Events;

namespace Core.Services;

public class PlanCommentService(IUnitOfWork unitOfWork,
                            IMapper mapper,
                            IGrpcTeamService grpcTeamService,
                            IGrpcPlanService grpcPlanService,
                            INotificationService notificationService,
                            IBackgroundService backgroundService) : IPlanCommentService
{
    private readonly IGenericRepository<PlanComment> _planCommentRepository = unitOfWork.GetRepository<PlanComment>();
    public async Task<Result<PlanCommentDto>> AddCommentAsync(AddPlanCommentDto addCommentDto)
    {
        // TODO: the main two validation
        // 1- check if plan exists 
        // 2- check if user is in the provided team by (userId) and (teamId) which is related to the plan
        var plan = await grpcPlanService.GetPlanIdsAsync(addCommentDto.PlanId!);
        if (!plan.IsExisted)
            return Result<PlanCommentDto>.Failure(PlanErrors.PlanNotFound);

        if (
            await grpcPlanService.IsOwnerOfPlanAsync(addCommentDto.UserId!, addCommentDto.PlanId!)
            ||
            await grpcTeamService.IsUserExist(addCommentDto.UserId!, plan.TeamId!))
        {
            var comment = mapper.Map<PlanComment>(addCommentDto);
            await _planCommentRepository.AddAsync(comment);
            await unitOfWork.SaveChangesAsync();
            var commentDto = mapper.Map<PlanCommentDto>(comment);

            // send notification to all team members
            backgroundService.ExecuteInBackground(() => SendNotificationAsync(plan, addCommentDto));

            return Result<PlanCommentDto>.Success(commentDto);
        }
        return Result<PlanCommentDto>.Failure(CommentErrors.AccessDenied);
    }
    public async Task<Result<IEnumerable<PlanCommentDto>>> GetCommentsByPlanIdAsync(string planId)
    {
        var isPlanExist = await grpcPlanService.GetPlanIdsAsync(planId);
        if (!isPlanExist.IsExisted)
            return Result<IEnumerable<PlanCommentDto>>.Failure(PlanErrors.PlanNotFound);

        var comments = await _planCommentRepository.FindAll(c =>
            c.PlanId == planId,
            orderBy: x => x.OrderByDescending(y => y.CreatedAt)
        );
        var commentDtos = mapper.Map<IEnumerable<PlanCommentDto>>(comments);
        return Result<IEnumerable<PlanCommentDto>>.Success(commentDtos);
    }
    public async Task<Result<PlanCommentDto>> UpdateCommentAsync(UpdatePlanCommentDto updateCommentDto)
    {
        var comment = await _planCommentRepository.Find(c => c.Id == updateCommentDto.CommentId);

        if (comment == null)
            return Result<PlanCommentDto>.Failure(CommentErrors.NotFound);
        if (comment.UserId != updateCommentDto.UserId)
            return Result<PlanCommentDto>.Failure(CommentErrors.AccessDenied);
        if (DateTime.UtcNow > comment.CreatedAt.AddMinutes(5))
            return Result<PlanCommentDto>.Failure(CommentErrors.EditTimeout);
        mapper.Map(updateCommentDto, comment);
        _planCommentRepository.Update(comment);
        await unitOfWork.SaveChangesAsync();

        var commentDto = mapper.Map<PlanCommentDto>(comment);
        return Result<PlanCommentDto>.Success(commentDto);
    }
    public async Task<Result<bool>> DeleteCommentAsync(string commentId, string userId)
    {
        var comment = await _planCommentRepository.Find(c => c.Id == commentId);

        if (comment == null)
            return Result<bool>.Failure(CommentErrors.NotFound);
        if (comment.UserId != userId)
            return Result<bool>.Failure(CommentErrors.AccessDenied);

        _planCommentRepository.Delete(comment);
        await unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
    private async Task SendNotificationAsync(PlanIdsDto plan, AddPlanCommentDto addCommentDto)
    {
        var teamMembersIds = await grpcTeamService.GetTeamMembersAsync(plan.TeamId!);

        foreach (var memberId in teamMembersIds)
        {
            if (memberId != addCommentDto.UserId)
            {
                var notificationForMember = new NotificationEventDto
                {
                    SenderId = addCommentDto.UserId!,
                    PlanId = addCommentDto.PlanId!,
                    Title = "New Comment Added",
                    Message = addCommentDto.UserComment!,
                    ReceiverId = memberId
                };
                await notificationService.SendNotificationAsync(notificationForMember);
            }
        }
    }
}
