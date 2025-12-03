using AutoMapper;
using Core.DTOs;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Mapping
{   
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<ChatMessage, ChatMessageResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TimeStamp, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<ChatMessage, CreateChatRequestDto>()
                .ReverseMap();

            CreateMap<ApiResponseDto, ChatMessageResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UserRole, opt => opt.Ignore())
                .ForMember(dest => dest.Query, opt => opt.Ignore())
                .ForMember(dest => dest.TimeStamp, opt => opt.Ignore())
                .ForMember(dest => dest.Response, opt => opt.MapFrom(src => src.Message))
                .ReverseMap();
        }
    }

}
