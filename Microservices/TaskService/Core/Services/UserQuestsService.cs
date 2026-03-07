using AutoMapper;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Core.Result;
using Core.ServicesAbstractions;

namespace Core.Services;

public class UserQuestsService(IUnitOfWork unitOfWork,
                          IMapper mapper) : IUserQuestsService
{
    private readonly IGenericRepository<Quest> _questRepository = unitOfWork.GetRepository<Quest>();
    private readonly IGenericRepository<UserQuests> _userQuestsRepository = unitOfWork.GetRepository<UserQuests>();
    public async Task<Result<QuestToReturnDto>> AddUserToQuestAsync(AddUserToQuestDto userToQuestDto)
    {
        var existedQuest = await _questRepository.Find(q => q.QuestId == userToQuestDto.QuestId, q => q.UserAssignedQuests);
        if (existedQuest is null)
        {
            return Result<QuestToReturnDto>.Failure(QuestErrors.QuestNotFound);
        }
        if (existedQuest.UserAssignedQuests.Any(uq => uq.UserId == userToQuestDto.UserId))
        {
            return Result<QuestToReturnDto>.Failure(UserQuestsErrors.UserAlreadyAssignedToQuest);
        }
        // TODO: here will be a grpc method that take userId and teamId and return boolean to check if the user exists in the team.
        existedQuest.UserAssignedQuests.Add(new UserQuests
        {
            QuestId = userToQuestDto.QuestId!,
            UserId = userToQuestDto.UserId!
        });
        await unitOfWork.SaveChangesAsync();
        var questToReturn = mapper.Map<QuestToReturnDto>(existedQuest);
        return Result<QuestToReturnDto>.Success(questToReturn);
    }
    public async Task<Result<IEnumerable<QuestToReturnDto>>> GetUserAssignedQuestsAsync(string userId)
    {
        var userQuests = await _userQuestsRepository.FindAll(uq => uq.UserId == userId, uq => uq.Quest);
        var questsToReturn = mapper.Map<IEnumerable<QuestToReturnDto>>(userQuests);
        return Result<IEnumerable<QuestToReturnDto>>.Success(questsToReturn);
    }
    public async Task<Result<bool>> UnAssignUserFromQuestAsync(string questId, string userId)
    {
        var existedUserQuest = await _userQuestsRepository.Find(uq => uq.QuestId == questId && uq.UserId == userId);
        if (existedUserQuest == null)
            return Result<bool>.Failure(UserQuestsErrors.UserNotAssignedToQuest);
        _userQuestsRepository.Delete(existedUserQuest);
        await unitOfWork.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
    // TODO: (HELPER METHOD) here will be a method that call grpc method to validate user existence TAKE: (userId, TeamId) retrun boolean.
}
