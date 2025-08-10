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
    public class TastingNoteService : ITastingNoteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint = "api/tastingnotes";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };

        public TastingNoteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TastingNoteDto>> GetAllTastingNotesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiEndpoint);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<IEnumerable<TastingNoteDto>>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting tasting notes: {ex.Message}");
                return new List<TastingNoteDto>();
            }
        }

        public async Task<IEnumerable<TastingNoteDto>> GetTastingNotesByBottleIdAsync(int bottleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/bottle/{bottleId}");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<IEnumerable<TastingNoteDto>>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting tasting notes for bottle {bottleId}: {ex.Message}");
                return new List<TastingNoteDto>();
            }
        }

        public async Task<TastingNoteDto> GetTastingNoteByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/{id}");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<TastingNoteDto>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting tasting note {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<TastingNoteDto> CreateTastingNoteAsync(TastingNoteCreateDto tastingNoteDto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(tastingNoteDto), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<TastingNoteDto>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tasting note: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateTastingNoteAsync(TastingNoteUpdateDto tastingNoteDto)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(tastingNoteDto), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiEndpoint}/{tastingNoteDto.Id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating tasting note: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTastingNoteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting tasting note {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<TastingNoteDto>> GetPublicTastingNotesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/public?page={page}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<IEnumerable<TastingNoteDto>>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting public tasting notes: {ex.Message}");
                return new List<TastingNoteDto>();
            }
        }
    }
}
