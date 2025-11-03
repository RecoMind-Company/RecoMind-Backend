using AutoMapper;
using Core.DTOs;
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

            CreateMap<CreateCompanyDTO, Core.Models.Company>()
                .ReverseMap();

            CreateMap<Core.Models.Company, UpdateCompanyDTO>()
                .ReverseMap();

            CreateMap<DeleteCompanyDTO, Core.Models.Company>()
                .ReverseMap();

            CreateMap<UpdateCompanyDTO, GetCompanyDTO>()
                .ReverseMap();
        }
        
    }
}
