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
                .ForMember(dest => dest.SubscriptionTypeName, opt => opt.MapFrom(src => src.subscriptionType.PlanName))
                .ReverseMap()
                // Explicitly handle the nested property for the reverse direction
                .ForPath(dest => dest.subscriptionType.PlanName, opt => opt.MapFrom(src => src.SubscriptionTypeName));

            CreateMap<SubscriptionCompany, DeleteSubscriptionCompanyDto>()
                .ReverseMap();
        }
    }
}
