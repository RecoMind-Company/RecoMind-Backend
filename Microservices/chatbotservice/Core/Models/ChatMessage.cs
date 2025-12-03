using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; }
        public string Query { get; set; }
        public string Response { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
