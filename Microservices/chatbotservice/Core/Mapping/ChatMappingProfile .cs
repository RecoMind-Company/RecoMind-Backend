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
            CreateMap<ChatMessage, CreateChatRequestDto>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Query, opt => opt.MapFrom(src => src.Query))
                 .ReverseMap()

                .ForMember(dest => dest.Id, opt => opt.Ignore())       
                .ForMember(dest => dest.TimeStamp, opt => opt.Ignore())
                .ForMember(dest => dest.Response, opt => opt.Ignore());

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
        }
    }

}
