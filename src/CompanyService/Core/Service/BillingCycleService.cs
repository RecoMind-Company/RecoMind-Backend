using AutoMapper;
using Core.Const;
using Core.DTOs;
using Core.Interfaces;
using Core.Service.Interface;
using Core.Models;
using Infrastructure.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service
{
    public class BillingCycleService : IBillingCycleServiice
    {
        private readonly IUnitOfWork<Core.Models.Company> _companyUnitOfWork;
        private readonly IMapper _mapper;
        public BillingCycleService(IUnitOfWork<Core.Models.Company> companyUnitOfWork, IMapper mapper)
        {
            _companyUnitOfWork = companyUnitOfWork;
            _mapper = mapper;
        }
        public async Task<GetCompanyDTO> AssignBillingCycle(string companyId, string cycleName)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

            if (string.IsNullOrWhiteSpace(cycleName))
                throw new ArgumentException("Billing cycle name cannot be null or empty.", nameof(cycleName));

            var company = await _companyUnitOfWork.Entity.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException($"Company with ID '{companyId}' not found.");

            if (!CheckBillingCycleName(cycleName))
                throw new ArgumentException($"Invalid billing cycle name: {cycleName}", nameof(cycleName));

            company.Billing = cycleName;
            await _companyUnitOfWork.Entity.UpdateAsync(company);

            return _mapper.Map<GetCompanyDTO>(company);
        }

        public bool CheckBillingCycleName(string cycleName)
        {
            if (string.IsNullOrWhiteSpace(cycleName))
                return false;

            return Enum.GetNames(typeof(BillingCycle))
                       .Any(name => string.Equals(name, cycleName, StringComparison.OrdinalIgnoreCase));
        }


        public IEnumerable<string> GetAllBillingCycles()
        {
           return Enum.GetNames(typeof(BillingCycle));
        }
    }
}
