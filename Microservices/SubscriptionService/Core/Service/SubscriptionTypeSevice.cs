using AutoMapper;
using Core.DTOs;
using Core.DTOs.SubscriptionTypeDto;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Service
{
    public class SubscriptionTypeSevice : ISubscriptionTypeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<SubscriptionType> _unitOfwork;

        public SubscriptionTypeSevice(IUnitOfWork<SubscriptionType> unitOfWork, IMapper mapper)
        {
            _unitOfwork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetDto> AddSubscriptionPlan(CreateDto planType)
        {
            var model = _mapper.Map<SubscriptionType>(planType);

            model.SubscriptionTypeId = Guid.NewGuid().ToString();

            await _unitOfwork.Entity.AddAsync(model);
            await _unitOfwork.Save();

            return _mapper.Map<GetDto>(model);
        }

        public async Task<bool> CheckPlanName(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());
            return result != null;
        }

        public async Task<DeleteDto> DeleteSubscriptionType(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());

            if (result == null)
                throw new ArgumentException($"Invalid {nameof(PlanName)}. Try again.");

            _unitOfwork.Entity.Delete(result);
            await _unitOfwork.Save();

            return new DeleteDto
            {
                PlanName = PlanName,
                Message = "Delete operation succeeded"
            };
        }

        public async Task<IEnumerable<GetDto>> GetAllSubscriptionPlan()
        {
            var result = await _unitOfwork.Entity.GetAllAsync();
            return _mapper.Map<IEnumerable<GetDto>>(result);
        }

        public async Task<string> GetId(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());

            if (result == null)
                throw new ArgumentException($"Invalid {nameof(PlanName)}. Try again.");

            return result.SubscriptionTypeId;
        }

        public async Task<double> GetPrice(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());

            if (result == null)
                throw new ArgumentException($"Invalid {nameof(PlanName)}. Try again.");

            return result.Price;
        }

        public async Task<GetDto> UpdateSubscriptionType(string oldPlantype, CreateDto planType)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == oldPlantype.ToLower());

            if (result == null)
                throw new ArgumentException($"Invalid {nameof(oldPlantype)}. Try again.");

            result.PlanName = planType.PlanName;
            result.Price = planType.Price;

            _unitOfwork.Entity.Update(result);
            await _unitOfwork.Save();

            return new GetDto
            {
                Id = result.SubscriptionTypeId,
                PlanName = result.PlanName,
                Price = result.Price
            };
        }
    }
}