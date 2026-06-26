using AutoMapper;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Core.Result;
using Core.ServicesAbstractions;
namespace Core.Services;

public class QuestService(IUnitOfWork unitOfWork,
                          IMapper mapper,
                          IGrpcModuleService grpcModuleService) : IQuestService
{
    private readonly IGenericRepository<Quest> _questRepository = unitOfWork.GetRepository<Quest>();
    public async Task<Result<QuestToReturnDto>> CreateQuestAsync(QuestDto questDto, string moduleId)
    {
        var isModuleExist = await grpcModuleService.GetmoduleIdsAsync(moduleId);
        if (!isModuleExist.IsExisted)
            return Result<QuestToReturnDto>.Failure(ModuleErrors.NotFound);

        var quest = mapper.Map<Quest>(questDto);
        quest.QuestId = Guid.NewGuid().ToString();
        quest.ModuleId = moduleId;
        await _questRepository.AddAsync(quest);
        await unitOfWork.SaveChangesAsync();
        var questToReturn = mapper.Map<QuestToReturnDto>(quest);
        return Result<QuestToReturnDto>.Success(questToReturn);
    }
    public async Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsAsync(string moduleId)
    {
        var quests = await _questRepository.FindAll(q => q.ModuleId == moduleId, q => q.UserAssignedQuests);
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
    public async Task<Result<IEnumerable<QuestToReturnDto>>> GetAllQuestsByStatusAsync(QuestByStatusDto questByStatusDto, string moduleId)
    {
        if (questByStatusDto.Status is null)
        {
            return await GetAllQuestsAsync(moduleId);
        }
        var quests = await _questRepository.FindAll(q => q.Status == (QuestStatusEnum)questByStatusDto.Status! && q.ModuleId == moduleId, q => q.UserAssignedQuests);
        var questsToReturn = mapper.Map<IEnumerable<QuestToReturnDto>>(quests);
        return Result<IEnumerable<QuestToReturnDto>>.Success(questsToReturn);
    }
    public async Task<Result<PersonalQuestToReturnDto>> CreatePersonalQuestAsync(QuestDto personalQuestDto)
    {
        var quest = mapper.Map<Quest>(personalQuestDto);
        quest.QuestId = Guid.NewGuid().ToString();
        await _questRepository.AddAsync(quest);
        await unitOfWork.SaveChangesAsync();

        var questToReturn = mapper.Map<PersonalQuestToReturnDto>(quest);
        return Result<PersonalQuestToReturnDto>.Success(questToReturn);
    }
}
