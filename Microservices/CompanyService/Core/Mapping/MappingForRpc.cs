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

            CreateMap<GetCompanyDTO, CompanyResponse>()
                 .ForMember(dest => dest.CreatedAt,
                  opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())))
                 .ReverseMap();      
            
            CreateMap<DeleteCompanyResponse, DeleteCompanyDTO>()
                .ForMember(dest => dest.Massage, opt=> opt.MapFrom(src=> src.Description))
                .ReverseMap();
        }
    }
}
