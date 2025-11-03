using AutoMapper;
using Core.Const;
using Core.DTOs;
using Core.Interfaces;
using Core.Service.Interface;
using Infrastructure.Service;
using Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service
{
    public class PlaneService : IPlaneService 
    {
        private readonly IUnitOfWork<Core.Models.Company> _companyUnitOfWork;
        private readonly IMapper _mapper;
        public PlaneService(IUnitOfWork<Core.Models.Company> companyUnitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _companyUnitOfWork = companyUnitOfWork;
        }
        public IEnumerable<string> GetAllPlans()
        {
            foreach (var item in Enum.GetValues(typeof(CompanyPlanType)))
            {
                yield return item.ToString();
            }
        }

        public async Task<GetCompanyDTO> AssignPlane(string Id, string PlaneName)
        {
            var item = await _companyUnitOfWork.Entity.GetByIdAsync(Id);
            if (item == null) throw new Exception("This Id Not Found ");

            if (!CheckPlanName(PlaneName)) throw new Exception("This Plan Not Found ");
            item.PlanType = PlaneName;


            await _companyUnitOfWork.Entity.UpdateAsync(item);

            return _mapper.Map<GetCompanyDTO>(item);
        }

        public bool CheckPlanName(string planName)
        {
            var plans = Enum.GetNames(typeof(CompanyPlanType)).Select(p => p.ToLower()).ToList();
            return plans.Contains(planName.ToLower());
        }
    
    }
}
