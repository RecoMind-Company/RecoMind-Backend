using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSetting.Core.Entities;

namespace DatabaseSetting.Core.Interfaces
{
    public interface IDbSettingRepository
    {
        Task<DbSettingModel> GetByCompanyIdAsync(string companyId);
        Task<DbSettingModel> GetByIdAsync(string Id);
        Task<DbSettingModel> CreateAsync(DbSettingModel model);
        Task<DbSettingModel> UpdateAsync(DbSettingModel model);
        Task<bool> DeleteAsync(DbSettingModel model);
    }
}
