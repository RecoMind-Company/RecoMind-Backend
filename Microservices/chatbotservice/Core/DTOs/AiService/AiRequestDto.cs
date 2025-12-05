using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.AiService
{
    public class AiRequestDto
    {
        public string compnay_id { get; set; }
        public string team_name { get; set; }
        public string user_question { get; set; }
    }
}
