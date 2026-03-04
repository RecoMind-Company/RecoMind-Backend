using Data_Mapping.Core.DTOs;
using Data_Mapping.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Core.Interfaces
{
    public interface IMappingRepository
    {
        Task<ClientSchemaVector?> GetByIdAsync(int id);
        Task<List<ClientSchemaVector>> GetByCompanyAsync(string companyId);
        // when click on Review, Return selected list of tables with assigned teams and description
        Task<List<ClientSchemaVector>> GetByDeptNameAsync(string companyId, string DeptName);
        Task<bool> UpdateAsync(ClientSchemaVector table);
    }
}
