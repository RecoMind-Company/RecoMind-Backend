using Core.DTOs.AiService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Interface
{
    public interface IAiClientService
    {
        public Task<FinalResponseDto> GetResponseFromAiService( string TasskId );
        public Task<AiResponseDto> SentRequestToAiService ( AiRequestDto ReqDto );        
    }
}
