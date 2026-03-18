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
    }


}
