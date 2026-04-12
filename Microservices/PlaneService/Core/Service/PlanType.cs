using Core.DTOs.PlnaTypeDtos;
using Core.Interfaces;
using Core.Service.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service
{
    public class PlanType : IPlanType
    {
        readonly IUnitOfWork<Core.Models.PlanType> _unitOfWork;
        public PlanType(IUnitOfWork<Models.PlanType> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPlanType(AddPlantypeDto Dto )
        {
            var planTypeEntity = new Core.Models.PlanType
            {
                Id = Guid.NewGuid().ToString(),
                Name = Dto.Name,
                NumOfMonths = Dto.NumOfMonths
            };
            await _unitOfWork.Entity.AddAsync(planTypeEntity);
            _unitOfWork.Save();
            return true;
        }

        public async Task<bool> DeletePlanType(string planType)
        {
            var isExist = GetPlanTypeByName(planType);

            if (isExist != null)
            {
                var plantype = new Core.Models.PlanType { Name=isExist.Result.Name };
                _unitOfWork.Entity.Delete(plantype);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<GetPlanTypeDto>> GetAllPlanTypes()
        {
            var result = new List<GetPlanTypeDto>(); 
            var items = await _unitOfWork.Entity.GetAllAsync();
            foreach (var item in items)
            {
                var itemToReturn = new GetPlanTypeDto { Name = item.Name , NumOfMonths = item.NumOfMonths};
                result.Add(itemToReturn);
            }
            return result;
        }

        public async Task<GetPlanTypeDto> GetPlanTypeByName(string planType)
        {
            var result = new GetPlanTypeDto();
            var item = await _unitOfWork.Entity.Find( x => x.Name.ToLower() == planType.ToLower());
            if (item == null)
                return null;
            else
            {
                result.Name=item.Name;
                result.NumOfMonths = item.NumOfMonths;
                return result;
            }
        }
    }
}
