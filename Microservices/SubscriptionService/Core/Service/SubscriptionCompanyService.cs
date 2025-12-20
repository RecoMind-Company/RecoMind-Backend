using AutoMapper;
using Core.Consts;
using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using System.Threading.Tasks;

namespace Core.Service
{
    public class SubscriptionCompanyService : ISubscriptionCompanyService
    {
        private readonly IUnitOfWork<SubscriptionCompany> _unitOfWork;
        private readonly ISubscriptionTypeService _subscriptionTypeService;
        private readonly IMapper _mapper;
        public SubscriptionCompanyService(IUnitOfWork<SubscriptionCompany> unitOfWork , IMapper mapper , ISubscriptionTypeService subscriptionTypeService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _subscriptionTypeService = subscriptionTypeService;
        }   

        public async Task<GetSubscriptionCompanyDto> CreateSubscription(CreateSubscriptionCompanyDto subscriptionDto)
        {
            if (subscriptionDto == null) throw new ArgumentNullException("Pleas Enter Subscribion Information ");

            var item = new SubscriptionCompany();

            item.Id = Guid.NewGuid().ToString();

            item.BillingCycle = checkBillingCycle(subscriptionDto.BillingCycle);

            item.SubscriptionTypeId = await _subscriptionTypeService.GetId(subscriptionDto.PlanName);
           
            item.Price = await SetPrice(subscriptionDto.BillingCycle, subscriptionDto.PlanName);

            item.EndDate = SetEndDate(subscriptionDto.BillingCycle);

            item.StartDate = DateTime.Now;

            item.IsActive = item.EndDate > item.StartDate ? true : false;

            await _unitOfWork.Entity.AddAsync(item);
            await _unitOfWork.Save();
            return _mapper.Map<GetSubscriptionCompanyDto>(item);
        }        

        public async Task<DeleteSubscriptionCompanyDto> DeleteSubscription(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) 
                throw new ArgumentNullException("Please Enter Subscribtion Id");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null)
                throw new KeyNotFoundException("Subscribtion Not Found ");

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
            if (string.IsNullOrEmpty(subscriptionId)) throw new ArgumentNullException("Please Enter Subscribtion Id");

            var item = await _unitOfWork.Entity.GetByIdAsync(subscriptionId);
            if (item == null) throw new KeyNotFoundException("Subscribtion Not Found");

            return _mapper.Map<GetSubscriptionCompanyDto>(item);
        }        

        public async Task<GetSubscriptionCompanyDto> UpdateSubscription(string subscriptionId, CreateSubscriptionCompanyDto subscriptionDto)
        {
            if (string.IsNullOrEmpty( subscriptionId) || subscriptionDto == null ) 
                throw new ArgumentNullException("Please Enter Required Feald");

            var item =await  _unitOfWork.Entity.GetByIdAsync(subscriptionId);

            if (item == null) throw new KeyNotFoundException("Subscribtion Not Found");

            item.BillingCycle = checkBillingCycle(subscriptionDto.BillingCycle);

            item.SubscriptionTypeId = await _subscriptionTypeService.GetId(subscriptionDto.PlanName);

            item.Price = await SetPrice(subscriptionDto.BillingCycle, subscriptionDto.PlanName);

            _mapper.Map(subscriptionDto, item);
            _unitOfWork.Entity.Update(item);
            await _unitOfWork.Save();

            return _mapper.Map<GetSubscriptionCompanyDto>(item);
        }       

        public async Task<double> SetPrice ( string billingCycle , string PlaneName  ) 
        {
            var price = await _subscriptionTypeService.GetPrice(PlaneName);

            switch (billingCycle.ToLower()) {
                case "monthly":
                    return price;

                case "annual":
                    return price * 12;

                case "semiannual":
                    return price * 6;

                default:
                    throw new ArgumentException(" Invalid Plane Name ");
            }
        }

        public DateTime SetEndDate(string BillingCycle)
        {
            switch (BillingCycle.ToLower())
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
        public string checkBillingCycle(string billingCycle)
        {
            if (string.IsNullOrWhiteSpace(billingCycle))
            {
                throw new ArgumentException("Billing cycle cannot be empty.", nameof(billingCycle));
            }

            BillingCycle result;

            bool isValid = Enum.TryParse(billingCycle, ignoreCase: true, out result);

            if (isValid)
            {                
                return result.ToString();                
            }

            var validNames = string.Join(", ", Enum.GetNames(typeof(BillingCycle)));

            throw new ArgumentException(
                $"Invalid Billing Cycle: '{billingCycle}'. Must be one of: {validNames} ",
                nameof(billingCycle));
        }
    }

}
