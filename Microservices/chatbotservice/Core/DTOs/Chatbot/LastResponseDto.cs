using Core.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Chatbot
{
    public class LastResponseDto
    {
        public Status status { get; set; }
        public string ResponseMessage { get; set; }
    }
}
