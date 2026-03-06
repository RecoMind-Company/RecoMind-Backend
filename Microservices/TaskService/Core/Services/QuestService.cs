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
    // TODO: (HELPER METHOD) here will be a method that call grpc method to validate plan existence.
}
