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


        CreateMap<Quest, QuestToReturnDto>();

        CreateMap<UpdateQuestDto, Quest>()
            .ForAllMembers(opt => opt.Condition((src, des, srcMember) => srcMember != null));
    }


}
