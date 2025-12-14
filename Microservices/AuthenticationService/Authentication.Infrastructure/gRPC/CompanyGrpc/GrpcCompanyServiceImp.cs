using Authentication.Core.Interfaces;
using Grpc.Core;

namespace Authentication.Infrastructure.gRPC.CompanyGrpc;

public class GrpcCompanyServiceImp(CompanyService.CompanyServiceClient companyClient) : IGrpcCompanyService
{
    public async Task<string> GetCompanyByUserId(string userId)
    {
        var request = new GitByAdminIdRequest { AdminId = userId };
        try
        {
            var response = await companyClient.GetCompanyByAdminIdAsync(request);
            return response.Id;
        }
        catch (RpcException ex)
        {
            return null;
        }
    }
}
