using AutoMapper;
using Core.Dtos.Task;
using Core.Models;

namespace Core.MappingProfiles;

public class QuestCommentProfile : Profile
{
    public QuestCommentProfile()
    {
        CreateMap<AddTaskCommentDto, QuestComment>()
            .ForMember(des => des.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(des => des.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<QuestComment, TaskCommentDto>();

        CreateMap<UpdateTaskCommentDto, QuestComment>()
            .ForMember(des => des.UserComment, opt =>
            {
                opt.Condition(src => !string.IsNullOrEmpty(src.UserComment));
                opt.MapFrom(src => src.UserComment);
            })
            .ForMember(des => des.Id, opt => opt.Ignore())
            .ForMember(des => des.UserId, opt => opt.Ignore())
            .ForMember(des => des.QuestId, opt => opt.Ignore())
            .ForMember(des => des.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
