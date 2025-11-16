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
            return Enum.GetNames(typeof(CompanyPlanType));
        }

        public async Task<GetCompanyDTO> AssignPlane(string companyId, string planeName)
        {
            if(string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentNullException(" Comapny Id cant Be Nuul Or Empty " ,nameof(companyId));

            if(string.IsNullOrWhiteSpace(planeName))
                throw new ArgumentNullException("Company Plane Name Cant Ba Null Or Empty ",nameof(planeName));

            if (!CheckPlanName(planeName))
                throw new KeyNotFoundException($" Invalid Plane Name {planeName} ");

            var company = await _companyUnitOfWork.Entity.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException($"This company Id  {companyId} Not Found ");

            company.PlanType = planeName;
            await _companyUnitOfWork.Entity.UpdateAsync(company);

            return _mapper.Map<GetCompanyDTO>(company);
        }

        public bool CheckPlanName(string planName)
        {
            if (string.IsNullOrWhiteSpace(planName))
                return false;

            return Enum.GetNames(typeof(CompanyPlanType))
                       .Any(name => string.Equals(name, planName, StringComparison.OrdinalIgnoreCase));
        }    
    }
}
