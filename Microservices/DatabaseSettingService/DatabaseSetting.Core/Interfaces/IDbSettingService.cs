using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Entities;
using DatabaseSetting.Core.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.Services
{
    public interface IDbSettingService
    {
        Task<Result<DbSettingResponseForAiDto>> GetByCompanyIdForAiAsync(string companyId);
        Task<Result<DbSettingResponseDto>> GetByCompanyIdAsync(string companyId);
        Task<Result<DbSettingResponseDto>> GetByIdAsync(string id, string companyId);

        Task<Result<DbSettingResponseDto>> CreateAsync(CreateDbSettingDto request, string companyId);
        Task<Result<DbSettingResponseDto>> UpdateAsync(string id, string companyId, UpdateDbSettingDto request);
        Task<Result<bool>> DeleteAsync(string id, string companyId);
    }
}
