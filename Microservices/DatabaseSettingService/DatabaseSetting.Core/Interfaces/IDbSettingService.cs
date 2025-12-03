using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.Services
{
    public interface IDbSettingService
    {
        Task<IEnumerable<DbSettingResponse>> GetAllByCompanyIdAsync(string companyId);
        Task<DbSettingResponse?> GetByIdAsync(string id, string companyId);
        Task<DbSettingModel> GetConnectionByIdAsync(string Id, string companyId);

        Task<DbSettingResponse> CreateAsync(CreateDbSettingModel request, string companyId);
        Task<DbSettingResponse> UpdateAsync(string id, string companyId, UpdateDbSettingModel request);
        Task<bool> DeleteAsync(string id, string companyId);

    }
}
