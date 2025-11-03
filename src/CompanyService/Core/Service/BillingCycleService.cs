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
        public async Task<GetCompanyDTO> AssignBillingCycle(string Id, string CycleName)
        {
            var item = await _companyUnitOfWork.Entity.GetByIdAsync(Id);
            if (item == null) throw new Exception("This Id Not Found ");

            if (!CheckBillingCycleName(CycleName)) throw new Exception("This Cycle Not Found ");
            item.Billing = CycleName;

            await _companyUnitOfWork.Entity.UpdateAsync(item);

            return _mapper.Map<GetCompanyDTO>(item);
        }

        public bool CheckBillingCycleName(string CycleName)
        {
            var Cycles = Enum.GetNames(typeof(BillingCycle)).Select(p => p.ToLower()).ToList();
            return CycleName.Contains(CycleName.ToLower());
        }

        public IEnumerable<string> GetAllBillingCycles()
        {
            foreach (var item in Enum.GetValues(typeof(BillingCycle)))           
                yield return item.ToString();            
        }       
    }
}
