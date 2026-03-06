using AutoMapper;
using Core.Dtos;
using Core.Models;

namespace Core.MappingProfiles;

public class QuestProfile : Profile
{
    public QuestProfile()
    {
        CreateMap<QuestDto, Quest>()
            .ForMember(des => des.StartDate, opt => opt.
            MapFrom(src => src.StartDate ?? DateTime.UtcNow))

            .ForMember(des => des.Status, opt => opt.
            MapFrom(src => src.Status != null
            ? (QuestStatusEnum)src.Status
            : (src.StartDate ?? DateTime.UtcNow).Date == DateTime.UtcNow.Date
            ? QuestStatusEnum.active
            : QuestStatusEnum.pending));


        CreateMap<Quest, QuestToReturnDto>()
            .ForMember(des => des.UserAssignedQuests, opt => opt.
            MapFrom(src => src.UserAssignedQuests.Select(x => x.UserId).ToList()));

        CreateMap<UserQuests, QuestToReturnDto>()
            .ForMember(des => des.QuestId, opt => opt.MapFrom(src => src.Quest.QuestId))
            .ForMember(des => des.Title, opt => opt.MapFrom(src => src.Quest.Title))
            .ForMember(des => des.Description, opt => opt.MapFrom(src => src.Quest.Description))
            .ForMember(des => des.Status, opt => opt.MapFrom(src => src.Quest.Status))
            .ForMember(des => des.StartDate, opt => opt.MapFrom(src => src.Quest.StartDate))
            .ForMember(des => des.DeadLine, opt => opt.MapFrom(src => src.Quest.DeadLine))
            .ForMember(des => des.Duration, opt => opt.MapFrom(src => src.Quest.Duration))
            .ForMember(des => des.PlanId, opt => opt.MapFrom(src => src.Quest.PlanId))

            .ForMember(des => des.UserAssignedQuests, opt => opt.
            MapFrom(src => src.Quest.UserAssignedQuests.Select(x => x.UserId).ToList()));

        CreateMap<UpdateQuestDto, Quest>()
            .ForAllMembers(opt => opt.Condition((src, des, srcMember) => srcMember != null));
    }


}
