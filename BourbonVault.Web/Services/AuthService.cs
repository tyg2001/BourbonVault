using BourbonVault.Core.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;

namespace BourbonVault.Web.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Login(LoginDto loginDto);
        Task<AuthResponseDto> Register(RegisterDto registerDto);
        Task Logout();
        Task<bool> CheckEmailExistsAsync(string email);
    }
    
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        
        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }
        
        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            
            if (response.IsSuccessStatusCode && result != null && result.Success)
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Token);
            }
            
            return result;
        }
        
        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            
            if (response.IsSuccessStatusCode && result != null && result.Success)
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Token);
            }
            
            return result;
        }
        
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        }
        
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var response = await _httpClient.GetAsync($"api/auth/check-email?email={Uri.EscapeDataString(email)}");
            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}
