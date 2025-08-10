using BourbonVault.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BourbonVault.Web.Services
{
    public class BottleService : IBottleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint = "api/bottles";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };

        public BottleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<BottleDto>> GetAllBottlesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiEndpoint);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<IEnumerable<BottleDto>>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bottles: {ex.Message}");
                return new List<BottleDto>();
            }
        }

        public async Task<BottleDto> GetBottleByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<BottleDto>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bottle {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<BottleDto> CreateBottleAsync(BottleCreateDto bottleDto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(bottleDto), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<BottleDto>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bottle: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateBottleAsync(BottleUpdateDto bottleDto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(bottleDto), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiEndpoint}/{bottleDto.Id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bottle: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBottleAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting bottle {id}: {ex.Message}");
                return false;
            }
        }
    }
}
