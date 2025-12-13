using Core.DTOs.AiService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Chatbot
{
    public class SaveDto : FinalResponseDto
    {
        public string UserQuestion { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; }
    }
}
