using Core.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class GetPlaneDto
    {
        public string Id { get; set; }
        public string? TeamId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
