using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class PlanService : IPlanService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<Plan> _unitOfWork;
        public PlanService(IMapper mapper , IUnitOfWork<Plan> unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<GetPlaneDto> CreatePlan(CreatePlanDto Plandto)
        {
            var model = new Plan();
            _mapper.Map(Plandto, model);

            model.Id = Guid.NewGuid().ToString();
            model.CreatedAt = DateTime.Now;
    
            if (model.TeamId != null)
            {
                //check is valid id from Team service by calling getteambyid 
            }

            var result = await _unitOfWork.Entity.AddAsync(model);
            await _unitOfWork.Save();
            return _mapper.Map(result,new GetPlaneDto());
        }

        public async Task<DeletePlanDto> DeletePlan(string PlanId)
        {
            if (string.IsNullOrEmpty(PlanId))
                throw new ArgumentNullException(nameof(PlanId));

            var plan = await _unitOfWork.Entity.GetByIdAsync(PlanId);

            if (plan == null) 
                throw new KeyNotFoundException(nameof(PlanId));

            var result = _unitOfWork.Entity.Delete(plan);
            await _unitOfWork.Save();

            return new DeletePlanDto { Id = result.Id , Message =" Plan Deleted Successfuly "};
        }

        public async Task<IEnumerable<GetPlaneDto>> GetAllPlans()
        {
            var result = await _unitOfWork.Entity.GetAllAsync();

            if (result is null || !result.Any())
                return Enumerable.Empty<GetPlaneDto>();

            return _mapper.Map<IEnumerable<GetPlaneDto>>(result);
        }

        public async Task<IEnumerable<GetPlaneDto>> GetAllPlansByTeamId(string TeamId)
        {
            var result = await _unitOfWork.Entity.FindAll( p=>p.TeamId == TeamId );

            if (result is null)
                return Enumerable.Empty<GetPlaneDto>();

            return _mapper.Map<IEnumerable<GetPlaneDto>>(result);
        }

        public async Task<GetPlaneDto> GetPlan(string PlanId)
        {
            var result = await _unitOfWork.Entity.GetByIdAsync(PlanId);

            if (result is null)
                throw new KeyNotFoundException( nameof(PlanId));

            return _mapper.Map<GetPlaneDto>(result);
        }

        public async Task<GetPlaneDto> UpdatePlan(string PlanId , CreatePlanDto Plandto)
        {
            var model = await _unitOfWork.Entity.GetByIdAsync(PlanId);

            if (model is null)
                throw new KeyNotFoundException(nameof(PlanId));
            
            var newPlan = _mapper.Map<Plan>(Plandto);
            newPlan.Id = PlanId ;  
            newPlan.CreatedAt =model.CreatedAt;
            newPlan.Status =model.Status;

            _unitOfWork.Entity.Update(newPlan);
            await _unitOfWork.Save();
          
            return   _mapper.Map<GetPlaneDto>(newPlan);
        }
    }
}
