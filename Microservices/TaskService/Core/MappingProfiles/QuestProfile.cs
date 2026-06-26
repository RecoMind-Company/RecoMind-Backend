using AutoMapper;
using Core.Dtos;
using Core.Dtos.AI;
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
            .ForMember(des => des.ModuleId, opt => opt.MapFrom(src => src.Quest.ModuleId))
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

        CreateMap<AITasksDto, Quest>()
            // 1. Map Explicit Name Mismatches
            .ForMember(dest => dest.QuestId, opt => opt.MapFrom(src => src.task_id))
            .ForMember(dest => dest.DeadLine, opt => opt.MapFrom(src => src.deadline_date))
            .ForMember(dest => dest.ModuleId, opt => opt.MapFrom(src => src.moduleId))

            // 2. Parse String Dates to DateTime
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateTime.Parse(src.start_date)))
            .ForMember(dest => dest.DeadLine, opt => opt.MapFrom(src => DateTime.Parse(src.deadline_date)))

            // 3. Map Enum (AutoMapper handles string-to-enum automatically if names match, 
            // but if they don't, you might need Enum.Parse)
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<QuestStatusEnum>(src.status, true)))

            // 4. Ignore the calculated/read-only property
            .ForMember(dest => dest.Duration, opt => opt.Ignore())

            // 5. Map the nested object into a Collection
            .ForMember(dest => dest.UserAssignedQuests, opt => opt.MapFrom(src =>
                src.suggested_owner != null
                    ? new List<UserQuests>
                      {
                          new UserQuests
                          {
                              QuestId = src.task_id,
                              UserId = src.suggested_owner.user_id
                          }
                      }
                    : new List<UserQuests>()));
    }
}
