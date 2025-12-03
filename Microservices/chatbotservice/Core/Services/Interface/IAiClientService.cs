using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Interface
{
    public interface IAiClientService
    {
        public Task<ApiResponseDto> GetAiResponse( string Query );
    }
}
