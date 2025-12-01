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
            CreateMap<createSubscriptionRequest, CreateSubscriptionDto>()
                 .ReverseMap();

            CreateMap<updateSubscriptionRequest, UpdateSubscriptionDto>()
                 .ReverseMap();

            CreateMap<GetSubscriptionDto, subscriptionResponse>()
                 .ForMember(dest => dest.StartDate,
                  opt => opt.MapFrom(src => Timestamp.FromDateTime(src.StartDate.ToUniversalTime())))

                 .ForMember(dest => dest.EndDate,
                  opt => opt.MapFrom(src => Timestamp.FromDateTime(src.EndDate.ToUniversalTime())))

                 .ReverseMap();

            CreateMap<deleteSubscriptionResponse, DeleteSubscriptionDto>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Msg))
                .ReverseMap();
        }
    }
}
