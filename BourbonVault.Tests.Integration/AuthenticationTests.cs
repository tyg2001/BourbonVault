using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BourbonVault.Core.DTOs;
using FluentAssertions;
using Xunit;

namespace BourbonVault.Tests.Integration
{
    public class AuthenticationTests : IClassFixture<ApiTestFixture>
    {
        private readonly ApiTestFixture _fixture;
        private readonly HttpClient _client;

        public AuthenticationTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var email = $"newuser_{Guid.NewGuid():N}@example.com";
            var registerRequest = new RegisterDto
            {
                Email = email,
                Username = $"newuser_{Guid.NewGuid():N}",
                Password = "Password123!",
                DisplayName = "New User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            if (response.StatusCode != HttpStatusCode.OK)
                await _fixture.LogResponseError(response, nameof(Register_WithValidCredentials_ShouldSucceed));
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldFail()
        {
            // Arrange
            var email = $"duplicate_{Guid.NewGuid():N}@example.com";
            
            // First registration
            var registerRequest1 = new RegisterDto
            {
                Email = email,
                Username = $"duplicateuser_{Guid.NewGuid():N}",
                Password = "Password123!",
                DisplayName = "Duplicate User"
            };
            
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest1);
            
            // Second registration with same email
            var registerRequest2 = new RegisterDto
            {
                Email = email,
                Username = $"anotheruser_{Guid.NewGuid():N}",
                Password = "DifferentPassword123!",
                DisplayName = "Another User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest2);
            if (response.StatusCode != HttpStatusCode.BadRequest)
                await _fixture.LogResponseError(response, nameof(Register_WithDuplicateEmail_ShouldFail));
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange - Register a user first
            var email = "logintest@example.com";
            var password = "Password123!";
            
            await _fixture.RegisterTestUserAsync(_client, email, password);
            
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            if (response.StatusCode != HttpStatusCode.OK)
                await _fixture.LogResponseError(response, nameof(Login_WithValidCredentials_ShouldSucceed));
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                await _fixture.LogResponseError(response, nameof(Login_WithInvalidCredentials_ShouldFail));
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task CheckEmail_WithExistingEmail_ShouldReturnTrue()
        {
            // Arrange - Register a user first
            var email = "emailcheck@example.com";
            await _fixture.RegisterTestUserAsync(_client, email);
            
            // Act
            var response = await _client.GetAsync($"/api/auth/check-email?email={email}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeTrue();
        }
        
        [Fact]
        public async Task CheckEmail_WithNonexistentEmail_ShouldReturnFalse()
        {
            // Arrange
            var email = "nonexistent-email@example.com";
            
            // Act
            var response = await _client.GetAsync($"/api/auth/check-email?email={email}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeFalse();
        }
    }
}
