using System.Diagnostics.Contracts;

namespace Core.Models
{
    public class AiServiceOptions
    {
        public string BaseUrl { get; set; }
        public string GetEndPoint { get; set; }
        public string PostEndPont { get; set; }
        public string ApiKey { get; set; }
    }
}