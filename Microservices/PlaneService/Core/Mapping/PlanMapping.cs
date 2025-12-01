using AutoMapper;
using Core.DTOs;
using Core.Models;


namespace Core.Mapping
{
    public class PlanMapping : Profile
    {
        public PlanMapping() 
        {
            CreateMap<CreatePlanDto, Plan>() 
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .AddTransform<string>(s => s == null ? string.Empty : s);
            
            CreateMap<GetPlaneDto , Plan>()
                .ReverseMap();
        }
    }
}
