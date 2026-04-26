using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Chatbot
{
    public class GetHistoryDto
    {
        public string Query { get; set; }
        public string ResponseMessage { get; set; }
    }
}
