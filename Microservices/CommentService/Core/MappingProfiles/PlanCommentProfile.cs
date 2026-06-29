using AutoMapper;
using Core.Dtos.Plan;
using Core.Models;

namespace Core.MappingProfiles;

public class PlanCommentProfile : Profile
{
    public PlanCommentProfile()
    {
        CreateMap<AddPlanCommentDto, PlanComment>()
            .ForMember(des => des.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(des => des.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<PlanComment, PlanCommentDto>();

        CreateMap<UpdatePlanCommentDto, PlanComment>()
            .ForMember(des => des.UserComment, opt =>
            {
                opt.Condition(src => !string.IsNullOrEmpty(src.UserComment));
                opt.MapFrom(src => src.UserComment);
            })
            .ForMember(des => des.Id, opt => opt.Ignore())
            .ForMember(des => des.UserId, opt => opt.Ignore())
            .ForMember(des => des.PlanId, opt => opt.Ignore())
            .ForMember(des => des.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }


}
