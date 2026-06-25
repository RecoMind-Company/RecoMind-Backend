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
            .ForMember(des => des.Duration, opt => opt
                .MapFrom(src => src.DeadLine - src.StartDate))
            .ForMember(des => des.UserAssignedQuests, opt => opt.
            MapFrom(src => src.UserAssignedQuests.Select(x => x.UserId).ToList()));

        CreateMap<Quest, PersonalQuestToReturnDto>()
            .ForMember(des => des.Duration, opt => opt
                .MapFrom(src => src.DeadLine - src.StartDate))
            .ForMember(des => des.UserAssignedQuests, opt => opt.
            MapFrom(src => src.UserAssignedQuests.Select(x => x.UserId).ToList()));

        CreateMap<UserQuests, QuestToReturnDto>()
            .ForMember(des => des.QuestId, opt => opt.MapFrom(src => src.Quest.QuestId))
            .ForMember(des => des.Title, opt => opt.MapFrom(src => src.Quest.Title))
            .ForMember(des => des.Description, opt => opt.MapFrom(src => src.Quest.Description))
            .ForMember(des => des.Status, opt => opt.MapFrom(src => src.Quest.Status))
            .ForMember(des => des.StartDate, opt => opt.MapFrom(src => src.Quest.StartDate))
            .ForMember(des => des.DeadLine, opt => opt.MapFrom(src => src.Quest.DeadLine))
            .ForMember(des => des.Duration, opt => opt.MapFrom(src => src.Quest.DeadLine - src.Quest.StartDate))
            .ForMember(des => des.PlanId, opt => opt.MapFrom(src => src.Quest.PlanId))
            .ForMember(des => des.UserAssignedQuests, opt => opt.
            MapFrom(src => src.Quest.UserAssignedQuests.Select(x => x.UserId).ToList()));

        CreateMap<UpdateQuestDto, Quest>()
         .ForMember(des => des.Title, opt =>
         {
             opt.Condition(src => src.Title != null);
             opt.MapFrom(src => src.Title!);
         })
         .ForMember(des => des.Description, opt =>
         {
             opt.Condition(src => src.Description != null);
             opt.MapFrom(src => src.Description!);
         })
         .ForMember(des => des.Status, opt =>
         {
             opt.Condition(src => src.Status.HasValue);
             opt.MapFrom(src => (QuestStatusEnum)src.Status.GetValueOrDefault());
         })
         .ForMember(des => des.StartDate, opt =>
         {
             opt.Condition(src => src.StartDate.HasValue);
             opt.MapFrom(src => src.StartDate.GetValueOrDefault());
         })
         .ForMember(des => des.DeadLine, opt =>
         {
             opt.Condition(src => src.DeadLine.HasValue);
             opt.MapFrom(src => src.DeadLine.GetValueOrDefault());
         });
    }
}
