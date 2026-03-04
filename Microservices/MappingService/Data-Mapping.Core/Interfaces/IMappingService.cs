using Data_Mapping.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Mapping.Core.Interfaces
{
    public interface IMappingService
    {
        Task<List<TableSummaryDto>> GetAvailableTablesAsync(string companyId, string deptName);
        Task<List<TableSummaryDto>> GetTablesByDeptAsync(string companyId, string deptName);
        Task<bool> AddTablesToDeptAsync(string companyId, string deptName, List<int> tableIds);
        Task<bool> RemoveTablesFromDeptAsync(string companyId, string deptName, List<int> tableIds);
    }
}
