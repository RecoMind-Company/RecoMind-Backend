using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class CreatePlanDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string? TeamId { get; set; }
    }
}
