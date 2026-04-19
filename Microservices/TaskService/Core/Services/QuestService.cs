using AutoMapper;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Core.Result;
using Core.ServicesAbstractions;
namespace Core.Services;

public class QuestService(IUnitOfWork unitOfWork,
                          IMapper mapper,
                          IGrpcPlanService grpcPlanService) : IQuestService
{
    private readonly IGenericRepository<Quest> _questRepository = unitOfWork.GetRepository<Quest>();
    public async Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto, string planId)
    {
        var isPlanExist = await grpcPlanService.GetPlanIdsAsync(planId);
        if (!isPlanExist.IsExisted)
            return Result<QuestToReturnDto>.Failure(PlanErrors.NotFound);

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
    public async Task<Result<bool>> DeleteQuestAsync(string questId)
    {
        var existedQuest = await _questRepository.Find(q => q.QuestId == questId);
        if (existedQuest is null)
            return Result<bool>.Failure(QuestErrors.QuestNotFound);
        _questRepository.Delete(existedQuest);
        await unitOfWork.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
    public async Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsByStatusAsync(QuestByStatusDto questByStatusDto, string planId)
    {
        if (questByStatusDto.Status is null)
        {
            return await GetAllQuestsAsync(planId);
        }
        var quests = await _questRepository.FindAll(q => q.Status == (QuestStatusEnum)questByStatusDto.Status! && q.PlanId == planId, q => q.UserAssignedQuests);
        var questsToReturn = mapper.Map<IEnumerable<QuestToReturnDto>>(quests);
        return Result<IEnumerable<QuestToReturnDto>>.Success(questsToReturn);
    }
}
