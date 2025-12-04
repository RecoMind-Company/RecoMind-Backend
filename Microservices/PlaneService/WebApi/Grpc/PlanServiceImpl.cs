using AutoMapper;
using Core.DTOs;
using Core.Service.Protos;
using Core.Services.Interface;
using Grpc.Core;
using Team.Grpc;
using static Team.Grpc.TeamGrpcService;

namespace WebApi.Grpc
{
    public class PlanServiceImpl : PlanService.PlanServiceBase
    {
        private readonly IPlanService _planService;
        private readonly IMapper _mapper;
        private readonly TeamGrpcServiceClient _teamGrpcServiceClient;
        public PlanServiceImpl(IPlanService planService , IMapper mapper , TeamGrpcServiceClient teamGrpcServiceClient)
        {
            _planService = planService;
            _mapper = mapper;
            _teamGrpcServiceClient = teamGrpcServiceClient;
        }

        public override async Task<PlanResponse> CreatePlan(CreatePlanRequest request,ServerCallContext context)
        { 
            var dto = _mapper.Map<CreatePlanDto>(request);

            if (!string.IsNullOrEmpty( request.TeamId ))
            {
                GetTeamByIdRequest teamId = new GetTeamByIdRequest() { TeamId = dto.TeamId };
                var validTeamId = _teamGrpcServiceClient.GetTeamBasicInfo(teamId);
                if (validTeamId == null)
                {
                    throw new KeyNotFoundException($"Team With Id {dto.TeamId} Not Found !");
                }
            }           
            var plan = await _planService.CreatePlan(dto);

            return _mapper.Map<PlanResponse>(plan);
        }

        public override async Task<PlanResponse> GetPlan(GetPlanRequest request, ServerCallContext context)
        {
            var plan = await _planService.GetPlan(request.Id);
            return _mapper.Map<PlanResponse>(plan);
        }

        public override async Task<DeletePlanResponse> DeletePlan(DeletePlanRequest request, ServerCallContext context)
        {
            var result = await _planService.DeletePlan(request.Id);

            return new DeletePlanResponse
            {
                Id = result.Id,
                Message = $" Plan {result.Id} Deleted successfuly "
            };
        }

        public override async Task<ListPlansResponse> GetPlanByTeamId(GetPlanByTeamIdRequest request, ServerCallContext context)
        {
            var plans = await _planService.GetAllPlansByTeamId(request.TeamId);

            var response = new ListPlansResponse();

            foreach (var plan in plans)
            {
                response.Plans.Add(_mapper.Map<PlanResponse>(plan));
            }
            return response;
        }       

        public override async Task<PlanResponse> UpdatePlan(UpdatePlanRequest request, ServerCallContext context)
        {
            var dto = _mapper.Map<CreatePlanDto>(request);
            if (!string.IsNullOrEmpty(request.TeamId))
            {
                GetTeamByIdRequest teamId = new GetTeamByIdRequest() { TeamId = dto.TeamId };
                var validTeamId = _teamGrpcServiceClient.GetTeamBasicInfo(teamId);
                if (validTeamId == null)
                {
                    throw new KeyNotFoundException($"Team With Id {dto.TeamId} Not Found !");
                }
            }
            var plan = await _planService.UpdatePlan(request.Id , dto);

            return _mapper.Map<PlanResponse>(plan);
        }

        public override async Task<ListPlansResponse> ListPlans( Empty request, ServerCallContext context)
        {
            var plans = await _planService.GetAllPlans();
            var response = new ListPlansResponse();
            foreach (var plan in plans)
            {
                response.Plans.Add(_mapper.Map<PlanResponse>(plan));
            }
            return response;
        }
    }
}
