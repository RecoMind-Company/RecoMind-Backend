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
        Task<DbSettingResponseForAiDto> GetByCompanyIdForAiAsync(string companyId);
        Task<DbSettingResponseDto> GetByCompanyIdAsync(string companyId);
        Task<DbSettingResponseDto> GetByIdAsync(string id, string companyId);

        Task<DbSettingResponseDto> CreateAsync(CreateDbSettingDto request, string companyId);
        Task<DbSettingResponseDto> UpdateAsync(string id, string companyId, UpdateDbSettingDto request);
        Task<bool> DeleteAsync(string id, string companyId);

    }
}
