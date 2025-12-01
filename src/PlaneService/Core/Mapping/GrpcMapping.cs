using AutoMapper;
using Core.DTOs;
using Core.Service.Protos;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Mapping
{
    public class GrpcMapping : Profile
    {
        public GrpcMapping()
        {
            CreateMap<CreatePlanRequest , CreatePlanDto>()
                .ReverseMap();

            CreateMap< GetPlaneDto, PlanResponse >()
                .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())))
                .ReverseMap();
        }
    }
}
