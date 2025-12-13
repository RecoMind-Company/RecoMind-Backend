using AutoMapper;
using Core.DTOs;
using Core.DTOs.SubscriptionTypeDto;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics.CodeAnalysis;

namespace Core.Service
{
    public class SubscriptionTypeSevice : ISubscriptionTypeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<SubscriptionType> _unitOfwork;
        public SubscriptionTypeSevice(IUnitOfWork<SubscriptionType> unitOfWork , IMapper mapper) 
        {
            _unitOfwork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<GetDto> AddSubscriptionPlan(CreateDto planType)
        {
            var model = _mapper.Map<SubscriptionType>(planType);

            model.SubscriptionTypeId=Guid.NewGuid().ToString();

            var result = await _unitOfwork.Entity.AddAsync(model);

            await _unitOfwork.Save();

            var dto = _mapper.Map<GetDto>(result);

            return dto;
        }

        public async Task<bool> CheckPlanName(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());

            if(result == null) 
                return false;
            return true;
        }

        public async Task<DeleteDto> DleteSubscriptionType(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());

            if (result == null)
                throw new ArgumentException($"Invalid {nameof(PlanName)} Try Again");

            _unitOfwork.Entity.Delete(result);

            await _unitOfwork.Save();

            return new DeleteDto { PlanName = PlanName , Message =$"Delete operation Successed"};
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
                throw new ArgumentException ($"Invalid {nameof(PlanName)} Try Again");

            return result.SubscriptionTypeId;
        }

        public async Task<double> GetPrice(string PlanName)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == PlanName.ToLower());

            if (result == null) 
                throw new ArgumentException($"Invalid {nameof(PlanName)} Try Again");

            return result.Price;
        }

        public async Task<GetDto> UpdateSubscriptionType(string oldPlantype, CreateDto planType)
        {
            var result = await _unitOfwork.Entity.Find(x => x.PlanName.ToLower() == oldPlantype.ToLower());

            if (result == null)
                throw new ArgumentException ($"Invalid Id : {nameof(oldPlantype)} Try Again");

            result.PlanName = planType.PlanName;
            result.Price = planType.Price;

            _unitOfwork.Entity.Update(result);
            await _unitOfwork.Save();

            return new GetDto { Id=result.SubscriptionTypeId, PlanName=result.PlanName,Price=result.Price };
        }
    }
}
