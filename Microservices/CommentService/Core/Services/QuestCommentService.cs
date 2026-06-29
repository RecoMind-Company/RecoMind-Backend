using AutoMapper;
using Core.Dtos.Task;
using Core.Interface;
using Core.Models;
using Core.Result;
using Core.ServicesAbstraction;

namespace Core.Services;

public class QuestCommentService(IUnitOfWork unitOfWork,
                                IMapper mapper,
                                IQuestGrpcService questGrpcService,
                                IUserQuestGrpcService userQuestGrpcService) : IQuestCommentService
{
    private readonly IGenericRepository<QuestComment> _questCommentsRepo = unitOfWork.GetRepository<QuestComment>();
    public async Task<Result<TaskCommentDto>> AddCommentAsync(AddTaskCommentDto addCommentDto)
    {
        var isTaskExisted = await questGrpcService.IsTaskExists(addCommentDto.QuestId);
        if (!isTaskExisted)
            return Result<TaskCommentDto>.Failure(TaskErrors.TaskNotFound);

        var isUserAssignedToTask = await userQuestGrpcService.IsUserAssignedToTask(addCommentDto.UserId!, addCommentDto.QuestId);
        if (!isUserAssignedToTask)
            return Result<TaskCommentDto>.Failure(TaskErrors.UserNotAssignedToTask);

        var comment = mapper.Map<QuestComment>(addCommentDto);
        await _questCommentsRepo.AddAsync(comment);
        await unitOfWork.SaveChangesAsync();

        var commentDto = mapper.Map<TaskCommentDto>(comment);
        return Result<TaskCommentDto>.Success(commentDto);
        // Proceed with adding the comment
    }

    public async Task<Result<IEnumerable<TaskCommentDto>>> GetCommentsByTaskIdAsync(string taskId)
    {
        var isTaskExisted = await questGrpcService.IsTaskExists(taskId);
        if (!isTaskExisted)
            return Result<IEnumerable<TaskCommentDto>>.Failure(TaskErrors.TaskNotFound);

        var comments = await _questCommentsRepo.FindAll(c => c.QuestId == taskId);
        var commentDtos = mapper.Map<IEnumerable<TaskCommentDto>>(comments);
        return Result<IEnumerable<TaskCommentDto>>.Success(commentDtos);
    }
    public async Task<Result<TaskCommentDto>> UpdateCommentAsync(UpdateTaskCommentDto updateCommentDto)
    {
        var comment = await _questCommentsRepo.Find(c => c.Id == updateCommentDto.CommentId);
        if (comment == null)
            return Result<TaskCommentDto>.Failure(CommentErrors.NotFound);
        if (comment.UserId != updateCommentDto.UserId)
            return Result<TaskCommentDto>.Failure(CommentErrors.AccessDenied);
        if (DateTime.UtcNow > comment.CreatedAt.AddMinutes(5))
            return Result<TaskCommentDto>.Failure(CommentErrors.EditTimeout);
        mapper.Map(updateCommentDto, comment);
        _questCommentsRepo.Update(comment);
        await unitOfWork.SaveChangesAsync();

        var commentDto = mapper.Map<TaskCommentDto>(comment);
        return Result<TaskCommentDto>.Success(commentDto);
    }

    public async Task<Result<bool>> DeleteCommentAsync(string commentId, string userId)
    {
        var comment = await _questCommentsRepo.Find(c => c.Id == commentId);

        if (comment == null)
            return Result<bool>.Failure(CommentErrors.NotFound);
        if (comment.UserId != userId)
            return Result<bool>.Failure(CommentErrors.AccessDenied);

        _questCommentsRepo.Delete(comment);
        await unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

}
