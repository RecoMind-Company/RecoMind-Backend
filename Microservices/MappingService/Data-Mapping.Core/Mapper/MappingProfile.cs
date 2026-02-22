using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data_Mapping.Core.DTOs;
using Data_Mapping.Core.Models;

namespace Data_Mapping.Core.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap <ClientSchemaVector, TableSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TableName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.TableDescription));
        }
    }
}
