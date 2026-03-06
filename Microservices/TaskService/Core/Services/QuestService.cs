using AutoMapper;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Core.Result;
using Core.ServicesAbstractions;
using Microsoft.EntityFrameworkCore;
namespace Core.Services;

public class QuestService(IUnitOfWork unitOfWork,
                          IMapper mapper) : IQuestService
{
    private readonly IGenericRepository<Quest> _questRepository = unitOfWork.GetRepository<Quest>();
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
        var quests = await _questRepository.GetAllAsync(q => q.UserAssignedQuests).Where(q => q.PlanId == planId).ToListAsync();
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
    // TODO: (HELPER METHOD) here will be a method that call grpc method to validate plan existence.
    // TODO: (HELPER METHOD) here will be a method that call grpc method to validate user existence TAKE: (userId, TeamId) retrun boolean.
}
