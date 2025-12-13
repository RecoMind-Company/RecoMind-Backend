using AutoMapper;
using Core.DTOs;
using Core.Models;
using Core.Service.Interface;
using Core.Service.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace WebApi.Grpc
{
    public class SubscriptionGrpcService : subscriptionService.subscriptionServiceBase
    {
        private readonly ISubscriptionCompanyService _service;
        private readonly IMapper _mapper;
        public SubscriptionGrpcService(ISubscriptionCompanyService service , IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        public override async Task<subscriptionResponse> create(createSubscriptionRequest request, ServerCallContext context)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var dto = new CreateSubscriptionCompanyDto();
            _mapper.Map(request, dto);

            await _service.CreateSubscription(dto);

           
            return _mapper.Map(dto,new subscriptionResponse());
        }

        public override async Task<subscriptionResponse> getById(getByIdRequest request, ServerCallContext context)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _service.GetSubscriptionById(request.Id);            
                   
            return _mapper.Map(result, new subscriptionResponse());
        }

        public override async Task<getAllSubscriptionResponse> getAll(empty request, ServerCallContext context)
        {
            var items = await _service.GetAllSubscriptions();

            var response = new getAllSubscriptionResponse();

            foreach (var item in items)
            {
                response.Subscriptions.Add(_mapper.Map(item, new subscriptionResponse()));
            }
            return response;
        }

        public override async Task<subscriptionResponse> update(updateSubscriptionRequest request, ServerCallContext context)
        {
            if ( request == null || string.IsNullOrWhiteSpace(request.Id) ) throw new ArgumentNullException(nameof(request));

            var item = await _service.GetSubscriptionById(request.Id);

            if (item == null) throw new KeyNotFoundException($"Subscription with Id {request.Id} not found.");

            var dto = new CreateSubscriptionCompanyDto();
            _mapper.Map(request, dto);
            var updatedItem = await _service.UpdateSubscription(request.Id, dto);
            return _mapper.Map(updatedItem, new subscriptionResponse());
        }

        public override async Task<deleteSubscriptionResponse> delete(getByIdRequest request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Id)) throw new ArgumentNullException(nameof(request.Id));

            var result = await _service.DeleteSubscription(request.Id);

            if (result == null) throw new KeyNotFoundException($"Subscription with Id {request.Id} not found.");  
            return _mapper.Map(result, new deleteSubscriptionResponse());
        }
    }
}
