using AutoMapper;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Models;
using Core.Services.Protos;

namespace Core.Mapping
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {

            CreateMap<ChatMessageResponse, FinalResponseDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status))

                .ForPath(dest => dest.Result.Answer, opt => opt.MapFrom(src => src.ResponseDetails.Answer))
                        .ForPath(dest => dest.Result.Sql_Query, opt => opt.MapFrom(src => src.ResponseDetails.SqlQuery))

                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId));

            CreateMap<HistoryEntry, GetHistoryDto>()
                .ReverseMap();

            CreateMap<FinalResponseDto, LastResponseDto>()
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.Result.Answer));


            CreateMap<FinalResponseDto, ChatMessage>()
                .ForMember(dest => dest.Response, opt => opt.MapFrom(src => src.Result))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UserRole, opt => opt.Ignore())
                .ForMember(dest => dest.UserQuestion, opt => opt.Ignore())
                .ForMember(dest => dest.TimeStamp, opt => opt.Ignore());

            //CreateMap<GetMethodDto, ChatMessage>()
            //    .ForMember(dest => dest.UserQuestion, opt => opt.MapFrom(src => src.user_question))
            //    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserID))
            //    .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.UserRole));                
        }
    }

}
