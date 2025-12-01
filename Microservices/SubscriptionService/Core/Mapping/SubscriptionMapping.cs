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
    public class SubscriptionMapping : Profile
    {
        public SubscriptionMapping() 
        {
            CreateMap< CreateSubscriptionDto ,Subscription>()
                .ForMember(dest => dest.Id , opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Subscription, UpdateSubscriptionDto>()
                .ReverseMap();

            CreateMap<Subscription, GetSubscriptionDto>()
                .ReverseMap();

            CreateMap<Subscription,DeleteSubscriptionDto>()
                .ReverseMap();
        }
    }
}
