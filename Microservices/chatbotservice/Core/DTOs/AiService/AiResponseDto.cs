using AutoMapper;
using Core.Const;
using Core.Interfaces;
using Core.Models;
using Core.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.AiService
{
    public class AiResponseDto
    {        
        public string task_id { get; set; }
        public Status status { get; set; }       
    }
}
