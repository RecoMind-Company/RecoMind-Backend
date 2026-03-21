using AutoMapper;
using Core.Dtos;
using Core.Models;

namespace Core.MappingProfiles;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<AddCommentDto, Comment>()
            .ForMember(des => des.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()));

        CreateMap<Comment, CommentDto>();

        CreateMap<UpdateCommentDto, Comment>()
            .ForMember(des => des.UserComment, opt =>
            {
                opt.Condition(src => !string.IsNullOrEmpty(src.UserComment));
                opt.MapFrom(src => src.UserComment);
            })
            .ForMember(des => des.Id, opt => opt.Ignore())
            .ForMember(des => des.UserId, opt => opt.Ignore())
            .ForMember(des => des.PlanId, opt => opt.Ignore());
    }


}
