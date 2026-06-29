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
            .ForMember(des => des.QuestId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))

            .ForMember(des => des.StartDate, opt => opt.
            MapFrom(src => src.StartDate ?? DateTime.UtcNow))

            .ForMember(des => des.Status, opt => opt.
            MapFrom(src => src.Status != null
            ? (QuestStatusEnum)src.Status
            : (src.StartDate ?? DateTime.UtcNow).Date == DateTime.UtcNow.Date
            ? QuestStatusEnum.in_progress
            : QuestStatusEnum.to_do))

            .ForMember(des => des.Priority, opt => opt.
            MapFrom(src => src.Priority != null
            ? (QuestPriorityEnum)src.Priority
            : QuestPriorityEnum.Low));

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
            .ForMember(dest => dest.QuestId, opt => opt.MapFrom(src => src.task_id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))

            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseStatus(src.status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => ParsePriority(src.priority)))

            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ParseDateTime(src.start_date)))
            .ForMember(dest => dest.DeadLine, opt => opt.MapFrom(src => ParseDateTime(src.deadline_date)))

            .ForMember(dest => dest.ModuleId, opt => opt.MapFrom((src, dest, destMember, context) =>
                context.Items.ContainsKey("ModuleId") ? context.Items["ModuleId"] as string : null))

            .ForMember(dest => dest.PlanId, opt => opt.MapFrom((src, dest, destMember, context) =>
            context.Items.ContainsKey("PlanId") ? context.Items["PlanId"] as string : null))

            .ForMember(dest => dest.UserAssignedQuests,
            opt => opt.MapFrom(
                    (src, dest) =>
                    src.suggested_owner != null && !string.IsNullOrEmpty(src.suggested_owner.user_id)
                        ? new List<UserQuests>
                        {
                            new UserQuests
                            {
                                QuestId = src.task_id,
                                UserId = src.suggested_owner.user_id
                            }
                        }
                        : new List<UserQuests>()
                    )
            )
            .ForMember(dest => dest.Duration, opt => opt.Ignore());
    }

    private static QuestStatusEnum ParseStatus(string statusStr)
    {
        return Enum.TryParse<QuestStatusEnum>(statusStr, true, out var parsedStatus) ? parsedStatus : default;
    }

    private static QuestPriorityEnum ParsePriority(string priorityStr)
    {
        return Enum.TryParse<QuestPriorityEnum>(priorityStr, true, out var parsedPriority) ? parsedPriority : default;
    }

    private static DateTime ParseDateTime(string dateStr)
    {
        return DateTime.TryParse(dateStr, out var parsedDate) ? parsedDate : DateTime.UtcNow;
    }

}
