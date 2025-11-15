using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.Interface
{
    public interface IBillingCycleServiice
    {
        public IEnumerable<string> GetAllBillingCycles();
        public bool CheckBillingCycleName(string CycleName);
        public Task<GetCompanyDTO> AssignBillingCycle(string Id, string CycleName);
    }
}
