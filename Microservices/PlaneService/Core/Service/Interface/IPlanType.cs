using Core.DTOs.PlnaTypeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface IPlanType
    {
        Task<bool> AddPlanType(AddPlantypeDto planType);
        Task<GetPlanTypeDto> GetPlanTypeByName(string planType);
        Task<IEnumerable<GetPlanTypeDto>> GetAllPlanTypes();
        Task<bool> DeletePlanType(string planType);
    }
}
