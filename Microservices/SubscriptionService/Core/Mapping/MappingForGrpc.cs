using AutoMapper;
using Core.DTOs;
using Core.Service.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Core.Mapping
{
    public class MappingForGrpc : Profile
    {
        public MappingForGrpc()
        {
            CreateMap<createSubscriptionRequest, CreateSubscriptionCompanyDto>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlaneName))
                .ForMember(dest => dest.BillingCycle, opt => opt.MapFrom(src => src.BillingCycle))
                .ReverseMap();

            CreateMap<updateSubscriptionRequest, UpdateSubscriptionCompanyDto>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.PlaneName))
                .ForMember(dest => dest.BillingCycle, opt => opt.MapFrom(src => src.BillingCycle))
                .ReverseMap();

            CreateMap<GetSubscriptionCompanyDto, subscriptionResponse>()
                 .ForMember(dest => dest.StartDate,
                  opt => opt.MapFrom(src => Timestamp.FromDateTime(src.StartDate.ToUniversalTime())))

                 .ForMember(dest => dest.EndDate,
                  opt => opt.MapFrom(src => Timestamp.FromDateTime(src.EndDate.ToUniversalTime())))                 

                 .ForMember(dest => dest.BillingCycle, opt => opt.MapFrom(src => src.BillingCycle))

                 .ReverseMap();

            CreateMap<deleteSubscriptionResponse, DeleteSubscriptionCompanyDto>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Msg))
                .ReverseMap();
        }
    }
}
