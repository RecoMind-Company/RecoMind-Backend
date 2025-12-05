using Core.DTOs.AiService;
using Core.Models;
using Core.Services.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Core.Services
{
    public class AiClientService : IAiClientService
    {
        private readonly HttpClient _http;
        private readonly string _endPoint;
        // private readonly string _apiKey;

        public AiClientService(HttpClient http, IOptions<AiServiceOptions> options)
        {
            _http = http;
            _endPoint = options.Value.BaseUrl;
            // _apiKey = options.Value.ApiKey;

            //if (!string.IsNullOrWhiteSpace(_apiKey))
            //{
            //    _http.DefaultRequestHeaders.Authorization =
            //        new AuthenticationHeaderValue("Bearer", _apiKey);
            //}
        }
        public async Task<FinalResponseDto> GetResponseFromAiService(string taskId)
        {
            
            var endpoint = $"{_endPoint}?taskId={taskId}";
            
            var response = await _http.GetAsync(endpoint);

            response.EnsureSuccessStatusCode(); 

            var resultObject = await response.Content.ReadFromJsonAsync<FinalResponseDto>();

            return resultObject!;
        }

        public async Task<AiResponseDto> SentRequestToAiService(AiRequestDto Dto)
        {
            var response = await _http.PostAsJsonAsync( _endPoint , Dto);

            response.EnsureSuccessStatusCode();

            var resultObject = await response.Content.ReadFromJsonAsync<AiResponseDto>();

            return resultObject!;
        }        
    }
}
