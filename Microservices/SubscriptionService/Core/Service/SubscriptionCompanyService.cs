using AutoMapper;
using Core.Consts;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Service
{
    public class SubscriptionCompanyService : ISubscriptionCompanyService
    {
        private readonly IUnitOfWork<SubscriptionCompany> _unitOfWork;
        private readonly ISubscriptionTypeService _subscriptionTypeService;
        private readonly IMapper _mapper;

        public SubscriptionCompanyService(
            IUnitOfWork<SubscriptionCompany> unitOfWork,
            IMapper mapper,
            ISubscriptionTypeService subscriptionTypeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _subscriptionTypeService = subscriptionTypeService;
        }

        public async Task<GetSubscriptionCompanyDto> CreateSubscription(CreateSubscriptionCompanyDto subscriptionDto)
        {
            if (subscriptionDto == null)
                throw new ArgumentNullException(nameof(subscriptionDto), "Please Enter Subscription Information");

            var item = new SubscriptionCompany
            {
                Id = Guid.NewGuid().ToString(),
                BillingCycle = checkBillingCycle(subscriptionDto.BillingCycle),
                SubscriptionTypeId = await _subscriptionTypeService.GetId(subscriptionDto.PlanName),
                Price = await SetPrice(subscriptionDto.BillingCycle, subscriptionDto.PlanName),
                StartDate = DateTime.UtcNow
            };

            item.EndDate = SetEndDate(subscriptionDto.BillingCycle);
            item.IsActive = item.EndDate > item.StartDate;

            await _unitOfWork.Entity.AddAsync(item);
            await _unitOfWork.Save();

            return _mapper.Map<GetSubscriptionCompanyDto>(item);
        }

        public async Task<DeleteSubscriptionCompanyDto> DeleteSubscription(string subscriptionId)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId), "Please Enter Subscription Id");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null)
                throw new KeyNotFoundException("Subscription Not Found");

            _unitOfWork.Entity.Delete(item);
            await _unitOfWork.Save();

            return _mapper.Map<DeleteSubscriptionCompanyDto>(item);
        }

        public async Task<IEnumerable<GetSubscriptionCompanyDto>> GetAllSubscriptions()
        {
            var items = await _unitOfWork.Entity.GetAllAsync();
            return _mapper.Map<IEnumerable<GetSubscriptionCompanyDto>>(items);
        }

        public async Task<GetSubscriptionCompanyDto> GetSubscriptionById(string subscriptionId)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
                throw new ArgumentNullException(nameof(subscriptionId), "Please Enter Subscription Id");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null)
                throw new KeyNotFoundException("Subscription Not Found");

            return _mapper.Map<GetSubscriptionCompanyDto>(item);
        }

        public async Task<GetSubscriptionCompanyDto> UpdateSubscription(string subscriptionId, CreateSubscriptionCompanyDto subscriptionDto)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId) || subscriptionDto == null)
                throw new ArgumentNullException("Please Enter Required Field");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null)
                throw new KeyNotFoundException("Subscription Not Found");

            _mapper.Map(subscriptionDto, item);

            item.BillingCycle = checkBillingCycle(subscriptionDto.BillingCycle);
            item.SubscriptionTypeId = await _subscriptionTypeService.GetId(subscriptionDto.PlanName);
            item.Price = await SetPrice(subscriptionDto.BillingCycle, subscriptionDto.PlanName);

            _unitOfWork.Entity.Update(item);
            await _unitOfWork.Save();

            return _mapper.Map<GetSubscriptionCompanyDto>(item);
        }

        public async Task<double> SetPrice(BillingCycle billingCycle, string planName)
        {
            var price = await _subscriptionTypeService.GetPrice(planName);

            return billingCycle switch
            {
                BillingCycle.Monthly => price,
                BillingCycle.Annual => price * 12,
                BillingCycle.SemiAnnual => price * 6,
                _ => throw new ArgumentOutOfRangeException(nameof(billingCycle))
            };
        }

        public DateTime SetEndDate(BillingCycle billingCycle)
        {
            return billingCycle switch
            {
                BillingCycle.Monthly => DateTime.UtcNow.AddMonths(1),
                BillingCycle.SemiAnnual => DateTime.UtcNow.AddMonths(6),
                BillingCycle.Annual => DateTime.UtcNow.AddYears(1),
                _ => throw new ArgumentException("Invalid Billing Cycle")
            };
        }

        public BillingCycle checkBillingCycle(BillingCycle billingCycle)
        {
            return billingCycle;
        }
    }
}