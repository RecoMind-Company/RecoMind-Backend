using System.Diagnostics.Contracts;

namespace Core.Models
{
    public class AiServiceOptions
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
        public string ApiKey { get; set; }
    }
}