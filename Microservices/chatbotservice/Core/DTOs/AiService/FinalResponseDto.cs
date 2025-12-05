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
        public string TaskId { get; set; }
        public ResponseMessage Response{ get; set; }
        public string Status { get; set; }
    }
    
    public class ResponseMessage
    {
        public string Sql_Query { get; set; }
        public string Answer { get; set; }
    }
}
