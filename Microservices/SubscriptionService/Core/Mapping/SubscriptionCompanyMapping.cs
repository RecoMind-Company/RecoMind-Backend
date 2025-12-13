using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Core.DTOs;
using Core.Models;
using Core.Service.Protos;

namespace Core.Mapping
{
    public class SubscriptionCompanyMapping : Profile
    {
        public SubscriptionCompanyMapping() 
        {
            CreateMap<CreateSubscriptionCompanyDto, SubscriptionCompany>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ReverseMap(); 

            CreateMap<SubscriptionCompany, UpdateSubscriptionCompanyDto>()
                .ReverseMap();

            CreateMap<SubscriptionCompany, GetSubscriptionCompanyDto>()
                .ReverseMap();

            CreateMap<SubscriptionCompany,DeleteSubscriptionCompanyDto>()
                .ReverseMap();
        }
    }
}
