using AutoMapper;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Models;
using Core.Services.Protos;
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

            CreateMap<ChatMessageResponse, FinalResponseDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status))

                .ForPath(dest => dest.Response.Answer, opt => opt.MapFrom(src => src.ResponseDetails.Answer))
                        .ForPath(dest => dest.Response.Sql_Query, opt => opt.MapFrom(src => src.ResponseDetails.SqlQuery))

                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId));

            CreateMap<HistoryEntry, GetHistoryDto>()
                .ReverseMap();

            CreateMap<FinalResponseDto, LastResponseDto>()
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.Response.Answer));

            //CreateMap<GetMethodDto, ChatMessage>()
            //    .ForMember(dest => dest.UserQuestion, opt => opt.MapFrom(src => src.user_question))
            //    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
            //    .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole));                
        }
    }

}
