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
            // AI Service -> Final DTO

            CreateMap<ChatMessageResponse, FinalResponseDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status))

                .ForPath(dest => dest.Result.Answer,
                    opt => opt.MapFrom(src => src.ResponseDetails.Answer))

                .ForPath(dest => dest.Result.Sql_Query,
                    opt => opt.MapFrom(src => src.ResponseDetails.SqlQuery))

                .ForMember(dest => dest.TaskId,
                    opt => opt.MapFrom(src => src.TaskId));



            // ChatMessage -> History DTO

            CreateMap<ChatMessage, GetHistoryDto>()
                .ForMember(dest => dest.Query,
                    opt => opt.MapFrom(src => src.UserQuestion))

                .ForMember(dest => dest.ResponseMessage,
                    opt => opt.MapFrom(src =>
                        src.Response != null
                            ? src.Response.Answer
                            : null));



            // HistoryEntry <-> GetHistoryDto

            CreateMap<HistoryEntry, GetHistoryDto>()
                .ReverseMap();



            // FinalResponseDto -> ChatMessage

            CreateMap<FinalResponseDto, ChatMessage>()
                .ForMember(dest => dest.Response,
                    opt => opt.MapFrom(src => src.Result))

                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())

                .ForMember(dest => dest.UserId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.UserRole,
                    opt => opt.Ignore())

                .ForMember(dest => dest.UserQuestion,
                    opt => opt.Ignore())

                .ForMember(dest => dest.TimeStamp,
                    opt => opt.Ignore());



            // ChatMessage -> SaveDto

            CreateMap<ChatMessage, SaveDto>()
                .ForPath(dest => dest.Result.Answer,
                    opt => opt.MapFrom(src =>
                        src.Response != null
                            ? src.Response.Answer
                            : null))

                .ForPath(dest => dest.Result.Sql_Query,
                    opt => opt.MapFrom(src =>
                        src.Response != null
                            ? src.Response.Sql_Query
                            : null))

                .ForMember(dest => dest.TaskId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.Status,
                    opt => opt.Ignore());



            // SaveDto -> ChatMessage

            CreateMap<SaveDto, ChatMessage>()
                .ForPath(dest => dest.Response.Answer,
                    opt => opt.MapFrom(src =>
                        src.Result != null
                            ? src.Result.Answer
                            : null))

                .ForPath(dest => dest.Response.Sql_Query,
                    opt => opt.MapFrom(src =>
                        src.Result != null
                            ? src.Result.Sql_Query
                            : null))

                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())

                .ForMember(dest => dest.UserId,
                    opt => opt.Ignore())

                .ForMember(dest => dest.UserRole,
                    opt => opt.Ignore())

                .ForMember(dest => dest.UserQuestion,
                    opt => opt.Ignore())

                .ForMember(dest => dest.TimeStamp,
                    opt => opt.Ignore());
        }
    }
}