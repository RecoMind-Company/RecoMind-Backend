using AutoMapper;
using Core.DTOs;
using Core.Service.Protos;
using Google.Protobuf.WellKnownTypes;

namespace webApi.Mapping
{
    public class MappingForRpc : Profile
    {
        public MappingForRpc()
        {
            CreateMap<CreateCompanyRequest, CreateCompanyDTO>()
                 .ReverseMap();

            CreateMap<UpdateCompanyRequest, UpdateCompanyDTO>()
                 .ForMember(dest => dest.Massage, opt => opt.MapFrom(src => src.Discription))
                 .ReverseMap();

            CreateMap<Core.DTOs.GetCompanyDTO, Core.Service.Protos.CompanyResponse>()

                .ForMember(dest => dest.SubscriptionId,
                            opt => opt.MapFrom(src => src.SubscriptionId ?? string.Empty))  
                
                .ForMember(dest => dest.CreatedAt,
                            opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())))

                .ForMember(dest => dest.Description,
                            opt => opt.MapFrom(src => src.Description?? string.Empty))

                .ForMember(dest => dest.Country,
                            opt => opt.MapFrom(src => src.Country ?? string.Empty))

                .ForMember(dest => dest.AdminId,
                            opt => opt.MapFrom(src => src.AdminId ?? string.Empty)
                );

            CreateMap<DeleteCompanyResponse, DeleteCompanyDTO>()
                .ForMember(dest => dest.Massage, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();
        }
    }
}
