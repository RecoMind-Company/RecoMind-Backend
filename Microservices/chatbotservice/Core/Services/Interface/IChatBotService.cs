using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Interface
{
    public interface IChatBotService
    {
        public Task<AiResponseDto> SendQuryToAiService(AiRequestDto requestDto);
        public Task<LastResponseDto> GetResponseFromAiService(GetMethodDto dto);
        public Task<IEnumerable<GetHistoryDto>> GetHistory(string userId);
        public Task DeleteHistory(string userId);

        //public Task<string> DeleteQuery(string queryId);
        //public Task<ChatMessageResponseDto> GetChatMessageById( string queryId);
    }
}
