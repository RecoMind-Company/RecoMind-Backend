using AutoMapper;
using Core.DTOs.PlanDtos;
using Core.Models;

namespace Core.Mapping
{
    public class PlanMapper : Profile
    {
        public PlanMapper()
        {
            CreateMap<AddPlanDto, Plan>()                
                .ForMember(dest => dest.Duration, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore())
                .ForMember(dest => dest.Owner_Id, opt => opt.Ignore())
                .ForMember(dest => dest.Company_Id, opt => opt.Ignore())
                .ForMember(dest => dest.Team_Id, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Plan, GetPlanDto>()
                .ForMember(dest => dest.PlanType, opt => opt.MapFrom(src => src.PlanType))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
        }
    }
}
