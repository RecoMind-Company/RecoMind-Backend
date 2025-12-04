using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;

namespace Core.Service
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork<Subscription> _unitOfWork;
        private readonly IMapper _mapper;
        public SubscriptionService(IUnitOfWork<Subscription> unitOfWork , IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }   

        public async Task<GetSubscriptionDto> CreateSubscription(CreateSubscriptionDto subscriptionDto)
        {
            if (subscriptionDto == null) throw new ArgumentNullException("Pleas Enter Subscribion Information ");

            var item = _mapper.Map(subscriptionDto,new Subscription());

            item.Id = Guid.NewGuid().ToString();
            item.Price = SetPrice(subscriptionDto.PlanName);
            item.StartDate= DateTime.Now;
            item.EndDate = SetEndDate(subscriptionDto.BillingCycle);
            item.IsActive = item.EndDate > item.StartDate ? true : false;

            await _unitOfWork.Entity.AddAsync(item);
            await _unitOfWork.Save();
            return _mapper.Map<GetSubscriptionDto>(item);
        }

        public async Task<DeleteSubscriptionDto> DeleteSubscription(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) 
                throw new ArgumentNullException("Please Enter Subscribtion Id");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null)
                throw new KeyNotFoundException("Subscribtion Not Found ");

            _unitOfWork.Entity.Delete(item);
            await _unitOfWork.Save();
            return _mapper.Map<DeleteSubscriptionDto>(item);
        }

        public async Task<IEnumerable<GetSubscriptionDto>> GetAllSubscriptions()
        {
            var items = await _unitOfWork.Entity.GetAllAsync();
            return _mapper.Map<IEnumerable<GetSubscriptionDto>>(items);
        }

        public async Task<GetSubscriptionDto> GetSubscriptionById(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) throw new ArgumentNullException("Please Enter Subscribtion Id");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);
            if (item == null) throw new KeyNotFoundException("Subscribtion Not Found");

            return _mapper.Map<GetSubscriptionDto>(item);
        }

        public async Task<UpdateSubscriptionDto> UpdateSubscription(string subscriptionId, CreateSubscriptionDto subscriptionDto)
        {
            if (string.IsNullOrEmpty( subscriptionId) || subscriptionDto == null ) 
                throw new ArgumentNullException("Please Enter Required Feald");

            var item =await  _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null) throw new KeyNotFoundException("Subscribtion Not Found");

            _mapper.Map(subscriptionDto, item);
            _unitOfWork.Entity.Update(item);
            await _unitOfWork.Save();

            return _mapper.Map<UpdateSubscriptionDto>(item);
        }       

        public double SetPrice ( string PlaneName) 
        {
            PlaneName = PlaneName.ToLower();
            switch (PlaneName) {
                case "free":
                    return 0;

                case "pro":
                    return 2000;

                case "enerprice":
                    return 5000;

                default:
                    throw new ArgumentException(" Invalid Plane Name ");
            }
        }

        public DateTime SetEndDate(string BillingCycle)
        {
            BillingCycle = BillingCycle.ToLower();
            switch (BillingCycle)
            {
                case "monthly":
                    return DateTime.Now.AddMonths(1);

                case "semiAnnual":
                    return DateTime.Now.AddMonths(6);

                case "annual":
                    return DateTime.Now.AddYears(1);

                default:
                    throw new ArgumentException(" Invalid Billing Cycle ");
            }
        }        
    }
}
