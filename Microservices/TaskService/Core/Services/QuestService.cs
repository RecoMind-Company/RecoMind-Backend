using AutoMapper;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Core.Result;
using Core.ServicesAbstractions;
namespace Core.Services;

public class QuestService(IUnitOfWork unitOfWork,
                          IMapper mapper) : IQuestService
{
    private readonly IGenericRepository<Quest> _questRepository = unitOfWork.GetRepository<Quest>();
    private readonly IGenericRepository<UserQuests> _userQuestsRepository = unitOfWork.GetRepository<UserQuests>();
    public async Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto, string planId)
    {
        // TODO: here will be a grpc method that take questDto.PlanId and return the plan to check if it exists.
        var quest = mapper.Map<Quest>(questDto);
        quest.QuestId = Guid.NewGuid().ToString();
        quest.PlanId = planId;
        await _questRepository.AddAsync(quest);
        await unitOfWork.SaveChangesAsync();
        var questToReturn = mapper.Map<QuestToReturnDto>(quest);
        return Result<QuestToReturnDto>.Success(questToReturn);
    }
    public async Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsAsync(string planId)
    {
        var quests = await _questRepository.FindAll(q => q.PlanId == planId, q => q.UserAssignedQuests);
        var questsToReturn = mapper.Map<IEnumerable<QuestToReturnDto>>(quests);
        return Result<IEnumerable<QuestToReturnDto>>.Success(questsToReturn);
    }
    public async Task<Result<QuestToReturnDto>> EditQuestAsync(UpdateQuestDto updateQuestDto, string questId)
    {
        var ExistedQuest = await _questRepository.Find(q => q.QuestId == questId);
        if (ExistedQuest is null)
            return Result<QuestToReturnDto>.Failure(QuestErrors.QuestNotFound);
        mapper.Map(updateQuestDto, ExistedQuest);
        await unitOfWork.SaveChangesAsync();
        var questToReturn = mapper.Map<QuestToReturnDto>(ExistedQuest);
        return Result<QuestToReturnDto>.Success(questToReturn);
    }
    public async Task<Result<QuestToReturnDto>> AddUserToQuestAsync(AddUserToQuestDto userToQuestDto)
    {
        var existedQuest = await _questRepository.Find(q => q.QuestId == userToQuestDto.QuestId, q => q.UserAssignedQuests);
        if (existedQuest is null)
        {
            return Result<QuestToReturnDto>.Failure(QuestErrors.QuestNotFound);
        }
        if (existedQuest.UserAssignedQuests.Any(uq => uq.UserId == userToQuestDto.UserId))
        {
            return Result<QuestToReturnDto>.Failure(QuestErrors.UserAlreadyAssignedToQuest);
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

    // TODO: (HELPER METHOD) here will be a method that call grpc method to validate plan existence.
    // TODO: (HELPER METHOD) here will be a method that call grpc method to validate user existence TAKE: (userId, TeamId) retrun boolean.
}
