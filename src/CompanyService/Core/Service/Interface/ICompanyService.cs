using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service.Interface
{
    public interface ICompanyService
    {
        public Task<IEnumerable<GetCompanyDTO>> GetAllCompaniesAsync();
        public Task<GetCompanyDTO> GetCompanyByIdAsync(string companyId);
        public Task<GetCompanyDTO> CreateCompanyAsync(CreateCompanyDTO createCompanyDTO);
        public Task<UpdateCompanyDTO> UpdateCompanyAsync(string companyId, CreateCompanyDTO companyDTO);
        public Task<DeleteCompanyDTO> DeleteCompanyAsync(string companyId);
    }
}
