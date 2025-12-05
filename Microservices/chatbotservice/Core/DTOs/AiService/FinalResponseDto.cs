using Core.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.AiService
{
    public class FinalResponseDto
    {
        public string Query { get; set; }
        public string ResponseMessage { get; set; }
        public Status Status { get; set; }
    }
}
