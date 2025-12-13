using AutoMapper;
using Core.DTOs.SubscriptionTypeDto;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Mapping
{
    public class SubscriptionTypeMapping : Profile
    {
        public SubscriptionTypeMapping()
        {
            CreateMap<SubscriptionType, CreateDto>()
                .ReverseMap();

            CreateMap<SubscriptionType, GetDto>()
                .ForMember( dst => dst.Id , opt=>opt.MapFrom(x=>x.SubscriptionTypeId))
                .ReverseMap();
        }
    }
}
