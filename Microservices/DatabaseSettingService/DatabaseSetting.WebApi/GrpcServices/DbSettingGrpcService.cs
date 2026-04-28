using DatabaseSetting.Core.Services;
using DatabaseSetting.WebApi.GrpcServices;
using Grpc.Core;
using DbSetting.Grpc;
using AutoMapper;

namespace DatabaseSetting.WebApi.GrpcServices
{
    public class DbSettingGrpcServiceImpl : DbSettingGrpcService.DbSettingGrpcServiceBase
    {
        private readonly IDbSettingService _service;
        public DbSettingGrpcServiceImpl(IDbSettingService service) => _service = service;

        public override async Task<DbSettingResponse> GetById(GetByIdRequest request, ServerCallContext context)
        {
            var result = await _service.GetByIdAsync(request.Id, request.CompanyId);

            return result.Map(
                res => new DbSettingResponse
                {
                    Id = res.Id,
                    CompanyId = res.CompanyId,
                    Name = res.Name,
                    DbType = res.DbType,
                    CreatedAt = res.CreatedAt.ToString("o")
                },
                error => throw new RpcException(new Status(StatusCode.NotFound, error.Message))
            );
        }

        public override async Task<DbSettingResponse> GetByCompanyId(GetByCompanyIdRequest request, ServerCallContext context)
        {
            var result = await _service.GetByCompanyIdAsync(request.CompanyId);

            return result.Map(
                res => new DbSettingResponse
                {
                    Id = res.Id,
                    CompanyId = res.CompanyId,
                    Name = res.Name,
                    DbType = res.DbType,
                    CreatedAt = res.CreatedAt.ToString("o")
                },
                error => throw new RpcException(new Status(StatusCode.NotFound, error.Message))
            );
        }
    }
}