using Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectApi.DTO
{
    public class CompanyDTO
    {

        public string Name { get; set; }
        public string? Industry { get; set; }

        public string? CompanySize { get; set; }
        public string? Country { get; set; }
        public string? CompanyCode { get; set; }
        public string? UserId { get; set; }

    }
}
