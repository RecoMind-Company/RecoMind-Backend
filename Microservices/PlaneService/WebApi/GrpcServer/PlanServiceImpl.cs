using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Grpc.Core;
using System.Threading.Tasks;

namespace webApi.Grpc
{
    public class PlanServiceImpl : PlanServiceGrpc.PlanServiceGrpcBase
    {
        public IUnitOfWork<Plan> _unitOfWork;
        public PlanServiceImpl(IUnitOfWork<Plan> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async override Task<PlanResponse> GetPlan(PlanRequest request, ServerCallContext context)
        {
            var plan = await _unitOfWork.Entity.GetByIdAsync(request.PlanId);

            if (plan != null)
            {
                return new PlanResponse
                {
                    IsExist = true,
                    CompanyId = plan.Company_Id,
                    OwnerId = plan.Owner_Id,
                    TeamId = plan.Team_Id,
                    PlanId = plan.Id,
                };
            }
            return new PlanResponse
            {
                IsExist = false,
            };
        }

        public async override Task<isOwnerResponse> isOwner(isOwnerRequest request, ServerCallContext context)
        {
            var plan = await _unitOfWork.Entity.GetByIdAsync(request.PlanId);
            if (plan != null && plan.Owner_Id == request.UserId)
            {
                return new isOwnerResponse
                {
                    IsOwner = true,
                };
            }
            return new isOwnerResponse
            {
                IsOwner = false,
            };
        }
    }
}
