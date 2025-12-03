using Core.DTOs;
using Core.Models;
using Core.Services.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Core.Services
{
    public class AiClientService : IAiClientService
    {
        private readonly HttpClient _http;
        private readonly string _aiBaseUrl;
        private readonly string _apiKey;

        public AiClientService(HttpClient http, IOptions<AiServiceOptions> options)
        {
            _http = http;
            _aiBaseUrl = options.Value.BaseUrl;
            _apiKey = options.Value.ApiKey;

            //if (!string.IsNullOrWhiteSpace(_apiKey))
            //{
            //    _http.DefaultRequestHeaders.Authorization =
            //        new AuthenticationHeaderValue("Bearer", _apiKey);
            //}
        }
        public async Task<ApiResponseDto> GetAiResponse(string Query)
        {
            
            var resp = await _http.PostAsJsonAsync($"{_aiBaseUrl}", Query);

            resp.EnsureSuccessStatusCode();

            var dto = await resp.Content.ReadFromJsonAsync<ApiResponseDto>()
                ?? new ApiResponseDto { Message = "" };

            return dto;
        }
    }
}
