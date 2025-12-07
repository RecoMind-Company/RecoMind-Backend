using Core.DTOs.AiService;
using Core.Models;
using Core.Services.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Services
{
    public class AiClientService : IAiClientService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _getEndPoint;
        private readonly string _postEndPoint;

        public AiClientService(IOptions<AiServiceOptions> options)
        {
            _baseUrl = options.Value.BaseUrl;
            _getEndPoint = options.Value.GetEndPoint;
            _postEndPoint = options.Value.PostEndPont;

            var handler = new HttpClientHandler();
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMinutes(30)
            };

            _http.DefaultRequestHeaders.Add("Accept", "application/json");
            _http.DefaultRequestHeaders.Add("User-Agent", "AiClientService");
        }

        public async Task<FinalResponseDto> GetResponseFromAiService(string taskId)
        {
            try
            {
                var endpoint = $"{_getEndPoint}/{taskId}";

                var response = await _http.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var resultObject = await response.Content.ReadFromJsonAsync<FinalResponseDto>();

                return resultObject!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while getting response from AI service: {ex.Message}", ex);
            }
        }

        public async Task<AiResponseDto> SentRequestToAiService(AiRequestDto dto)
        {            
            var requestDto = new AiRequestDto
            {
                company_id = "fb140d33-7e96-474d-a06d-ab3a6c65d1a9",
                team_name = "Sales",
                user_question = dto.user_question
            };

            // تحويل إلى JSON
            var response = await _http.PostAsJsonAsync(_postEndPoint, requestDto);

            response.EnsureSuccessStatusCode();

            var analysisResponse = await response.Content.ReadFromJsonAsync<AiResponseDto>();

            analysisResponse.user_question = dto.user_question;

            return analysisResponse!;
        }
    }
}
