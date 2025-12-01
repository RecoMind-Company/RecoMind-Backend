using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Entities;
using DatabaseSetting.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.Services
{
    public class DbSettingService : IDbSettingService
    {
        private readonly IDbSettingRepository _repository;
        private readonly IEncryptionService _encryptionService;
        public DbSettingService(IDbSettingRepository repository, IEncryptionService encryptionService)
        {
            _repository = repository;
            _encryptionService = encryptionService;
        }


        public async Task<IEnumerable<DbSettingResponse>> GetAllByCompanyIdAsync(string companyId)
        {
            var entities = await _repository.GetAllByCompanyIdAsync(companyId);

            if (entities == null || !entities.Any())
                return new List<DbSettingResponse>();

            return entities.Select(entity => new DbSettingResponse
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                Name = entity.Name,
                DbType = entity.DbType,
                CreatedAt = entity.CreatedAt
            });
        }

        public async Task<DbSettingResponse?> GetByIdAsync(string id, string companyId)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);

            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        public async Task<DbSettingModel> GetConnectionByIdAsync(string id, string companyId)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);

            if (entity == null)
                return null;

            entity.ConnectionString = _encryptionService.Decrypt(entity.ConnectionString);

            return entity;
        }


        public async Task<DbSettingResponse> CreateAsync(CreateDbSettingModel request)
        {
            var model = new DbSettingModel
            {
                Id = Guid.NewGuid().ToString(),
                CompanyId = request.CompanyId,
                Name = request.Name,
                DbType = request.DbType,
                ConnectionString = _encryptionService.Encrypt(request.ConnectionString),
                CreatedAt = DateTime.UtcNow
            };

            var saved = await _repository.CreateAsync(model);

            return new DbSettingResponse
            {
                Id = saved.Id,
                CompanyId = saved.CompanyId,
                Name = saved.Name,
                DbType = saved.DbType,
                CreatedAt = saved.CreatedAt
            };
        }

        public async Task<DbSettingResponse?> UpdateAsync(string id, string companyId, UpdateDbSettingModel request)
        {
            var entity = await _repository.GetByIdAsync(id, companyId);

            if (entity == null)
                return null;

            entity.Name = request.Name;
            entity.DbType = request.DbType;

            if (!string.IsNullOrWhiteSpace(request.ConnectionString) &&
                request.ConnectionString != _encryptionService.Decrypt(entity.ConnectionString))
            {
                entity.ConnectionString = _encryptionService.Encrypt(request.ConnectionString);
            }

            var updated = await _repository.UpdateAsync(entity);

            return MapToResponse(updated);
        }


        public async Task<bool> DeleteAsync(string id, string companyId)
        {
            return await _repository.DeleteAsync(id, companyId);
        }



        private DbSettingResponse MapToResponse(DbSettingModel entity) => new DbSettingResponse
        {
            Id = entity.Id,
            CompanyId = entity.CompanyId,
            Name = entity.Name,
            DbType = entity.DbType,
            CreatedAt = entity.CreatedAt
        };
    }
}
