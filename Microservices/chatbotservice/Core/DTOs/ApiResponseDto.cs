using AutoMapper;
using Core.Interfaces;
using Core.Models;
using Core.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ApiResponseDto
    {        
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
