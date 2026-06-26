using AutoMapper;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Core.Result;
using Core.ServicesAbstractions;
using RecoMind.Contracts.Events;

namespace Core.Services;

public class UserQuestsService(IUnitOfWork unitOfWork,
                          IMapper mapper,
                          IGrpcTeamService grpcTeamService,
                          INotificationService notificationService) : IUserQuestsService
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

        var userExistsInTeam = await grpcTeamService.IsUserExist(userToQuestDto.UserId!, userToQuestDto.TeamId!);
        if (!userExistsInTeam)
        {
            return Result<QuestToReturnDto>.Failure(UserQuestsErrors.UserNotInTeam);
        }

        existedQuest.UserAssignedQuests.Add(new UserQuests
        {
            QuestId = userToQuestDto.QuestId!,
            UserId = userToQuestDto.UserId!
        });
        await unitOfWork.SaveChangesAsync();

        var questToReturn = mapper.Map<QuestToReturnDto>(existedQuest);

        var notification = new NotificationEventDto
        {
            PlanId = existedQuest.ModuleId,
            Title = "New Quest Assigned",
            Message = $"You have been assigned to the quest: {existedQuest.Title}",
            ReceiverId = userToQuestDto.UserId!
        };
        await notificationService.SendNotificationAsync(notification);

        return Result<QuestToReturnDto>.Success(questToReturn);
    }
    public async Task<Result<PersonalQuestToReturnDto>> AssignUsersToQuestAsync(List<string> userIds, string questId)
    {
        //var usersExistInTeam = await Task.WhenAll(userIds.Select(userId => grpcTeamService.IsUserExist(userId, teamId)));
        //if (usersExistInTeam.Any(exists => !exists))
        //{
        //    return Result<QuestToReturnDto>.Failure(UserQuestsErrors.UserNotInTeam);
        //}
        var quest = await _questRepository.Find(q => q.QuestId == questId, q => q.UserAssignedQuests);
        if (quest is null)
        {
            return Result<PersonalQuestToReturnDto>.Failure(QuestErrors.QuestNotFound);
        }

        foreach (var userId in userIds)
        {
            quest!.UserAssignedQuests.Add(new UserQuests
            {
                QuestId = quest.QuestId,
                UserId = userId
            });
        }
        await unitOfWork.SaveChangesAsync();
        var questToReturn = mapper.Map<PersonalQuestToReturnDto>(quest);
        return Result<PersonalQuestToReturnDto>.Success(questToReturn);
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
    public async Task<bool> IsUserAssignedToAnyQuestInPlan(string userId, string planId)
    {
        var isExist = await _questRepository.AnyAsync(q => q.ModuleId == planId && q.UserAssignedQuests.Any(uq => uq.UserId == userId));
        return isExist;
    }
}
