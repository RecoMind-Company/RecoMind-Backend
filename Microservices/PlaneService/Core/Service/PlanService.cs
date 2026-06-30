using AutoMapper;
using Core.DTOs.AI;
using Core.DTOs.PlanDtos;
using Core.DTOs.PlanDtos.Approve;
using Core.DTOs.PlanDtos.Plan;
using Core.DTOs.Quest;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Core.Service.Interface.AI;
using Infrastructure.GrpcClients.Team;
using RecoMind.Contracts.Events;

namespace Core.Service
{
    public class PlanService : IPlanService
    {
        readonly ITeamGrpcClient _teamGrpcClient;
        readonly IUnitOfWork<Plan> _unitOfWork;
        readonly IPlanType _planTypeService;
        readonly IStatus _statusService;
        readonly IMapper _mapper;
        readonly IPlanEventPublisher _planEventPublisher;
        readonly IPlanGeneratorService _planGeneratorService;
        readonly IQuestGrpcClient _questGrpcClient;
        readonly IBackgroundService _backgroundService;
        public PlanService(IUnitOfWork<Plan> planUnitOfWork,
            IMapper mapper,
            IStatus StatusService,
            IPlanType PlanTypeService,
            ITeamGrpcClient TeamGrpcCleint,
            IPlanEventPublisher planEventPublisher,
            IPlanGeneratorService planGeneratorService,
            IQuestGrpcClient questGrpcClient,
            IBackgroundService backgroundService)
        {
            _unitOfWork = planUnitOfWork;
            _mapper = mapper;
            _planTypeService = PlanTypeService;
            _statusService = StatusService;
            _teamGrpcClient = TeamGrpcCleint;
            _planEventPublisher = planEventPublisher;
            _planGeneratorService = planGeneratorService;
            _questGrpcClient = questGrpcClient;
            _backgroundService = backgroundService;
        }

        public async Task<Result<GetPlanDto>> CreatePlan(AddPlanDto createPlanDto, string companyId, string userId)
        {

            Plan plan = new Plan();
            plan = _mapper.Map<Plan>(createPlanDto);

            plan.Id = Guid.NewGuid().ToString();
            plan.Owner_Id = userId;
            plan.Company_Id = companyId;
            plan.Status = "Draft";                                           // Default status for new plans
            plan.IsApproved = false;                                         // Default approval status for new plans            
            plan.StartDate = DateTime.UtcNow;                                // Default start date for new plans

            var checkEndDate = await GetPlanEndDate(plan.PlanType);         // Calculate end date based on plan type

            if (checkEndDate.IsSuccess)
            {
                plan.EndDate = checkEndDate.Value;
            }
            else
                return Result<GetPlanDto>.Failure(checkEndDate.Error);

            plan.Duration = (plan.EndDate - plan.StartDate).Days.ToString();  // Calculate duration in days

            var checkTeamId = await _teamGrpcClient.GetTeamNameById(userId);  //Check if the user is part of a team and get the team id
            if (checkTeamId.IsSuccess)
            {
                plan.Team_Id = checkTeamId.Value;
            }
            else
                return Result<GetPlanDto>.Failure(checkTeamId.Error);

            await _unitOfWork.Entity.AddAsync(plan);
            _unitOfWork.Save();

            // Sending the notification with English text
            await _planEventPublisher.PublishNotificationAsync(new NotificationEventDto
            {
                Title = "New Plan Created",
                Message = "A new plan has been successfully created !",
                ReceiverId = userId,
                SenderId = "System",
                PlanId = plan.Id
            });

            var result = _mapper.Map<GetPlanDto>(plan);

            return Result<GetPlanDto>.Success(result);
        }

        public async Task<Result<DateTime>> GetPlanEndDate(string planType)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(planType))
            {
                return Result<DateTime>.Failure("Invalid plan type provided.");
            }

            // Implement logic to calculate end date based on plan type
            // For example, if planType is "Monthly", add 30 days to the current date
            planType = planType.ToLower();
            DateTime endDate = DateTime.UtcNow;

            // find the plantype
            var planTypeEntity = await _planTypeService.GetPlanTypeByName(planType);
            if (planTypeEntity == null)
            {
                return Result<DateTime>.Failure("Invalid plan type provided.");
            }
            else
            {
                endDate = endDate.AddMonths(planTypeEntity.NumOfMonths);
                return Result<DateTime>.Success(endDate);
            }

        }

        public async Task<bool> DeletePlan(string planId, string companyId)
        {
            var plan = await _unitOfWork.Entity.Find(p => p.Id == planId && p.Company_Id == companyId);
            if (plan == null)
            {
                return false; // Plan not found
            }

            _unitOfWork.Entity.Delete(plan);
            _unitOfWork.Save();

            return true; // Plan deleted successfully
        }

        public async Task<IEnumerable<Result<GetPlanDto>>> GetAllPlans(string companyId)
        {
            var plans = await _unitOfWork.Entity.FindAll(p => p.Company_Id == companyId);

            var planToReturn = new List<Result<GetPlanDto>>();

            if (plans != null)
            {
                foreach (var plan in plans)
                {
                    var item = _mapper.Map<GetPlanDto>(plan);
                    planToReturn.Add(Result<GetPlanDto>.Success(item));
                }
                return planToReturn;
            }
            return planToReturn; // No plans found for the company
        }

        public async Task<Result<GetPlanDto>> GetPlanById(string planId, string companyId)
        {
            var plan = await _unitOfWork.Entity.Find(
                p => p.Company_Id == companyId && p.Id == planId,
                p => p.Modules
            );

            if (plan != null)
            {
                var item = _mapper.Map<GetPlanDto>(plan);
                return Result<GetPlanDto>.Success(item);
            }
            return Result<GetPlanDto>.Failure("Invalid PLan Id !");
        }

        public async Task<Result<GetPlanDto>> UpdatePlan(string companyId, string userId, UpdatePlanDto updatePlanDto)
        {
            var plan = await _unitOfWork.Entity.Find(p => p.Id == updatePlanDto.PlanId && p.Company_Id == companyId);

            if (plan != null)
            {

                plan.Goal = updatePlanDto.Goal;
                plan.Description = updatePlanDto.Description;

                #region ckeck for the name of plan type 

                if (!string.IsNullOrWhiteSpace(updatePlanDto.PlanType))
                {
                    var planTypeEntity = await _planTypeService.GetPlanTypeByName(updatePlanDto.PlanType);
                    if (planTypeEntity != null)
                    {
                        if (!string.Equals(plan.PlanType, updatePlanDto.PlanType, StringComparison.OrdinalIgnoreCase))
                        {
                            plan.EndDate = plan.StartDate.AddMonths(planTypeEntity.NumOfMonths); // Update end date based on new plan type
                            plan.Duration = (plan.EndDate - plan.StartDate).Days.ToString(); // Update duration based on new end date
                            plan.PlanType = updatePlanDto.PlanType;
                        }
                    }
                    else
                    {
                        return Result<GetPlanDto>.Failure("Invalid plan type provided.");
                    }
                }
                #endregion

                #region check for the name of status

                var statusEntity = await _statusService.GetStatusByName(updatePlanDto.Status);

                if (statusEntity != null)
                {
                    plan.Status = updatePlanDto.Status;
                }
                else
                {
                    return Result<GetPlanDto>.Failure("Invalid status provided.");
                }

                #endregion

                #region check for the team id

                var checkTeamId = await _teamGrpcClient.GetTeamNameById(userId);  //Check if the user is part of a team and get the team id
                if (checkTeamId.IsSuccess)
                {
                    plan.Team_Id = checkTeamId.Value;
                }
                else
                    return Result<GetPlanDto>.Failure(checkTeamId.Error);

                #endregion                

                var result = await _unitOfWork.Entity.UpdateAsync(plan);
                _unitOfWork.Save();

                await _planEventPublisher.PublishNotificationAsync(new NotificationEventDto
                {
                    Title = " Plan Updated Successfuly",
                    Message = "An plan has been successfully Updated !",
                    ReceiverId = userId,
                    SenderId = "System",
                    PlanId = plan.Id
                });

                var dto = _mapper.Map<GetPlanDto>(result);
                return Result<GetPlanDto>.Success(dto);
            }

            return Result<GetPlanDto>.Failure("Invalid Plan Id !");
        }

        public async Task<IEnumerable<Result<GetPlanDto>>> GetPlansByStatus(string status, string companyId)
        {
            var planToRetyrn = new List<Result<GetPlanDto>>();
            var items = await _unitOfWork.Entity.FindAll((x => (x.Company_Id == companyId && x.Status == status)));

            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    var dto = _mapper.Map<GetPlanDto>(item);
                    planToRetyrn.Add(Result<GetPlanDto>.Success(dto));
                }
                return planToRetyrn;
            }
            planToRetyrn.Add(Result<GetPlanDto>.Failure($"No plans found with the specified status : {status}"));
            return planToRetyrn;
        }

        public async Task<Result<AIPlanDto>> CreateCustomPlan(AIGetPlanDto aIGetPlanDto)
        {
            var response = await _planGeneratorService.GetPlanResult(aIGetPlanDto.TaskId);
            if (!response.IsSuccess)
                return Result<AIPlanDto>.Failure(response.Error);

            var plan = new Plan();

            plan.IsApproved = false;
            plan.Company_Id = aIGetPlanDto.CompanyId;

            //var checkTeamId = await _teamGrpcClient.GetTeamNameById(aIGetPlanDto.UserId);  //Check if the user is part of a team and get the team id
            //if (!checkTeamId.IsSuccess)
            //    return Result<AIPlanDto>.Failure(checkTeamId.Error);

            plan.Team_Id = "a875858b-83ce-4034-9c5a-6b83359b9bb8";


            plan.Owner_Id = aIGetPlanDto.UserId;


            _mapper.Map(response.Value, plan);

            await _unitOfWork.Entity.AddAsync(plan);
            _unitOfWork.Save();

            // IN BACKGROUND JOBS (Hangfire) CALL A gRPC service that will take a list of tasks and add them to DB
            var postModuleTasksDtos = response.Value.modules.Select(x => new PostModuleTasksDto
            {
                tasksDto = x.tasks,
                moduleId = x.module_id
            });

            var postTasks = new PostTasksDto
            {
                PlanId = plan.Id,
                ModulesTasks = postModuleTasksDtos
            };
            _backgroundService.ExecuteInBackground(() => _questGrpcClient.PostTasksToQuestService(postTasks));

            return Result<AIPlanDto>.Success(response.Value);
        }

        public async Task<Result<RequestCustomPlanResponseDto>> RequestCustomPlan(UserCustomPlanDto userCustomPlanDto, string companyId, string userId)
        {
            //var checkTeamId = await _teamGrpcClient.GetTeamNameById(userId);  //Check if the user is part of a team and get the team id
            //if (!checkTeamId.IsSuccess)
            //    return Result<RequestCustomPlanResponseDto>.Failure(checkTeamId.Error);

            var request = new AIRequestDto
            {
                company_id = companyId,
                plan_text = userCustomPlanDto.Description,
                team_id = "0dc1400d-a758-424b-80fb-a8ff89078522"
            };
            var result = await _planGeneratorService.GeneratePlan(request);
            return result;

        }
        public async Task<Result<PostIsApprovedDto>> IsApproved(PostIsApprovedDto approvedDto)
        {
            var plan = await _unitOfWork.Entity.Find(p => p.Id == approvedDto.PlanId);
            if (plan == null) // If the plan is not found, return a failure result
                return Result<PostIsApprovedDto>.Failure("Plan Not Found!");

            if (plan.IsApproved) // If the plan is already approved, return a message indicating that   
            {
                return Result<PostIsApprovedDto>.Success(new PostIsApprovedDto
                {
                    PlanId = plan.Id,
                    IsAproved = plan.IsApproved,
                    Feedback = "Plan With Id: " + plan.Id + " Is Already Approved!"
                });
            }
            else if (plan.Feedback != null && !plan.IsApproved) // If The Plan Already Rejected 
            {
                return Result<PostIsApprovedDto>.Failure("Plan With Id: " + plan.Id + " Is Already Rejected!");
            }
            else if (plan.Feedback == null && !plan.IsApproved)  // If the plan is not approved, update the approval status and feedback
            {

                if (approvedDto.IsAproved)
                {
                    plan.IsApproved = approvedDto.IsAproved;
                    plan.Feedback = approvedDto.Feedback ?? "Plan Approved Successfully!";

                }
                else
                {
                    plan.IsApproved = approvedDto.IsAproved;
                    plan.Feedback = approvedDto.Feedback ?? "Plan Rejected!";

                    await _questGrpcClient.DeleteTaskByPlanId(plan.Id); // Delete all tasks related to the plan if it is rejected            
                }
                await _unitOfWork.Entity.UpdateAsync(plan);
                _unitOfWork.Save();
                return Result<PostIsApprovedDto>.Success(new PostIsApprovedDto
                {
                    PlanId = plan.Id,
                    IsAproved = plan.IsApproved,
                    Feedback = plan.Feedback
                });
            }
            else
            {
                return Result<PostIsApprovedDto>.Failure("Invalid Plan Approval Status!");
            }
        }
    }
}
