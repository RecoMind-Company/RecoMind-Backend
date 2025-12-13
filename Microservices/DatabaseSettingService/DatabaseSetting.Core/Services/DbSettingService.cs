using AutoMapper;
using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Entities;
using DatabaseSetting.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DatabaseSetting.Core.Services
{
    public class DbSettingService : IDbSettingService
    {
        private readonly IDbSettingRepository _repository;
        private readonly IEncryptionService _encryp;
        private readonly IMapper _mapper;

        public DbSettingService(IDbSettingRepository repository, IEncryptionService encryptionService, IMapper mapper)
        {
            _repository = repository;
            _encryp = encryptionService;
            _mapper = mapper;
        }


        public async Task<DbSettingResponseForAiDto?> GetByCompanyIdForAiAsync(string companyId)
        {
            var entity = await _repository.GetByCompanyIdAsync(companyId);
            if (entity == null)
                return null;

            var response = _mapper.Map<DbSettingResponseForAiDto>(entity);
            response.Password = _encryp.Decrypt(entity.Password);

            return response;
        }

        public async Task<DbSettingResponseDto?> GetByCompanyIdAsync(string companyId)
        {
            var entity = await _repository.GetByCompanyIdAsync(companyId);
            if (entity == null)
                return null;

            return _mapper.Map<DbSettingResponseDto>(entity);
        }
        
        public async Task<DbSettingResponseDto?> GetByIdAsync(string id, string companyId)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.CompanyId != companyId) 
                return null;

            return _mapper.Map<DbSettingResponseDto>(entity);
        }


        public async Task<DbSettingResponseDto> CreateAsync(CreateDbSettingDto request, string companyId)
        {
            var model = _mapper.Map<DbSettingModel>(request);

            model.Id = Guid.NewGuid().ToString();
            model.CompanyId = companyId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            model.Password = _encryp.Encrypt(request.Password);

            var saved = await _repository.CreateAsync(model);
            return _mapper.Map<DbSettingResponseDto>(saved);
        }

        public async Task<DbSettingResponseDto?> UpdateAsync(string id, string companyId, UpdateDbSettingDto request)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.CompanyId != companyId)
                return null;

            _mapper.Map(request, entity);

            if (!string.IsNullOrEmpty(request.Password))
                entity.Password = _encryp.Encrypt(request.Password);

            entity.UpdatedAt = DateTime.UtcNow;
            var saved = await _repository.UpdateAsync(entity);
            return _mapper.Map<DbSettingResponseDto>(saved);
        }

        public async Task<bool> DeleteAsync(string id, string companyId)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.CompanyId != companyId)
                return false;

            return await _repository.DeleteAsync(entity);
        }
    }
}
