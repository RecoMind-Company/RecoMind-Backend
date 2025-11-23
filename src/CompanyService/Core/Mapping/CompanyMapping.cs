using AutoMapper;
using Core.DTOs;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Configuration
{
    public class CopmanyMapping : Profile
    {
        public CopmanyMapping() 
        {
            CreateMap<Core.Models.Company, GetCompanyDTO>()
                .ForMember(st=>st.BillingCycle, opt=>opt.MapFrom(src=>src.Billing))
                .ReverseMap();

            CreateMap<CreateCompanyDTO, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Core.Models.Company, UpdateCompanyDTO>()
                .ReverseMap();

            CreateMap<DeleteCompanyDTO, Company>()
                .ReverseMap();            
        }        
    }
}
