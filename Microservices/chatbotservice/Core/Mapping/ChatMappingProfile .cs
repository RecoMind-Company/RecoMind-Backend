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
                .ReverseMap();
       
            // Mapping between CreateChatRequestDto and CreateChatRequest (gRPC)

            CreateMap<CreateChatRequestDto, CreateChatRequest>();

            CreateMap<ChatMessageResponse,FinalResponseDto>()
                .ReverseMap();

            CreateMap<chatHistory, GetHistoryDto>()
                .ReverseMap();

        }
    }

}
