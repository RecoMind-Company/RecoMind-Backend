using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
    }
}
