using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Interface
{
    public interface IChatBotService
    {
        public Task<ChatMessageResponseDto> HandelQuery(CreateChatRequestDto requestDto);
        public Task<string> DeleteQuery(string queryId);
        public Task<ChatMessageResponseDto> GetChatMessageById( string queryId);
    }
}
