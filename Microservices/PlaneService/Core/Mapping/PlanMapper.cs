using AutoMapper;
using Core.DTOs.AI;
using Core.DTOs.PlanDtos.Module;
using Core.DTOs.PlanDtos.Plan;
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

            CreateMap<Module, GetModuleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.Id) && src.Id.Contains('_')
                        ? src.Id.Substring(0, src.Id.LastIndexOf('_'))
                        : src.Id));

            CreateMap<Plan, GetPlanDto>()
                .ForMember(dest => dest.PlanType, opt => opt.MapFrom(src => src.PlanType))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.IsApproved))
                .ForMember(dest => dest.Feedback, opt => opt.MapFrom(src => src.Feedback))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Modules))
                .ReverseMap();

            // AI Mapping 
            // AI Plan
            CreateMap<AIPlanDto, Plan>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.plan_id))
                .ForMember(dest => dest.Goal, opt => opt.MapFrom(src => src.plan_title))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.deadline_date))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.total_estimated_days.ToString()))

                // Those are nullable
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.PlanType, opt => opt.Ignore())
                // Those will be add by me.
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.Owner_Id, opt => opt.Ignore())
                .ForMember(dest => dest.Company_Id, opt => opt.Ignore())
                .ForMember(dest => dest.Team_Id, opt => opt.Ignore());

            // 2. AI Module
            CreateMap<AIModuleDto, Module>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.module_id + $"_{Guid.NewGuid().ToString()}"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.module_name))

                .ForMember(dest => dest.PlanId, opt => opt.Ignore())
                .ForMember(dest => dest.Plan, opt => opt.Ignore());
        }
    }
}
