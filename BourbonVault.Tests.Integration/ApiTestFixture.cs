using BourbonVault.API;
using BourbonVault.Core.DTOs;
using BourbonVault.Core.Models;
using BourbonVault.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Xunit;

namespace BourbonVault.Tests.Integration
{
    /// <summary>
    /// API test fixture that uses an in-memory database for integration testing
    /// </summary>
    public class ApiTestFixture : WebApplicationFactory<TestProgram>, IAsyncLifetime
    {
        // Using a unique database name for each test run to avoid conflicts
        private readonly string _databaseName = $"BourbonVault_InMemory_{Guid.NewGuid()}";
        
        public ApiTestFixture() { }
        
        // IAsyncLifetime implementation
        public Task InitializeAsync() => Task.CompletedTask;
        
        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
        
        /// <summary>
        /// Helper method to log detailed error information from HTTP responses
        /// </summary>
        public async Task LogResponseError(HttpResponseMessage response, string context)
        {
            Console.WriteLine($"HTTP Error in '{context}': {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            
            string content = "[No content]";
            try
            {
                content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading response content: {ex.Message}");
            }
        }
        
        // WebApplicationFactory override
        public override ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
        
        protected override IHostBuilder CreateHostBuilder() =>
            TestProgram.CreateHostBuilder(Array.Empty<string>())
                .ConfigureLogging(logging =>
                {
                    // Add console logging to see errors during tests
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    // Add JWT settings for tests
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["JwtSettings:Secret"] = "TestSecretKeyForIntegrationTestsOnly123456789012345678901234567890123456789012345",
                        ["JwtSettings:Issuer"] = "BourbonVaultAPI",
                        ["JwtSettings:Audience"] = "BourbonVaultClients",
                        ["IsTestEnvironment"] = "true"
                    });
                });

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Configure for testing environment
            builder.UseEnvironment("Testing");
            
            // Additional configuration for the test host
            builder.ConfigureServices(services =>
            {
                // Ensure we use our test database name consistently
                // Find and remove any existing DbContext registrations
                var dbContextDescriptor = services.SingleOrDefault(d => 
                    d.ServiceType == typeof(DbContextOptions<BourbonVaultContext>));
                
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);
                    
                // Remove any other EF Core registrations that might be conflicting
                var optionsDescriptors = services.Where(d => 
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.IsGenericType && 
                     d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))).ToList();
                     
                foreach (var descriptor in optionsDescriptors)
                    services.Remove(descriptor);
                
                // Register in-memory database with our test database name
                services.AddDbContext<BourbonVaultContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                // Build the service provider and seed baseline data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<BourbonVaultContext>();
                db.Database.EnsureCreated();

                // Seed default Distilleries used by tests and search expectations
                if (!db.Distilleries.Any())
                {
                    db.Distilleries.AddRange(
                        new BourbonVault.Core.Models.Distillery
                        {
                            Id = 1,
                            Name = "Buffalo Trace Distillery",
                            Location = "Frankfort, KY",
                            Region = "Kentucky",
                            Description = "Seeded for integration tests",
                            Website = "https://www.buffalotracedistillery.com/",
                            ImageUrl = "https://example.com/btd.jpg",
                            IsActive = true,
                            YearFounded = 1773
                        },
                        new BourbonVault.Core.Models.Distillery
                        {
                            Id = 2,
                            Name = "Woodford Reserve Distillery",
                            Location = "Versailles, KY",
                            Region = "Kentucky",
                            Description = "Seeded for integration tests",
                            Website = "https://www.woodfordreserve.com/",
                            ImageUrl = "https://example.com/wrd.jpg",
                            IsActive = true,
                            YearFounded = 1812
                        }
                    );
                    db.SaveChanges();
                }
            });
        }

        /// <summary>
        /// Seed the database with test data
        /// </summary>
        private void SeedTestData(BourbonVaultContext dbContext)
        {
            // Add test data here if needed
        }

        /// <summary>
        /// Helper method to authenticate a user and get a JWT token
        /// </summary>
        public async Task<string> GetAuthTokenAsync(HttpClient client, string email, string password)
        {
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            });

            loginResponse.EnsureSuccessStatusCode();
            
            var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            return authResult.Token;
        }

        /// <summary>
        /// Helper to create a unique email for test users
        /// </summary>
        public string GetUniqueEmail() => $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
        
        /// <summary>
        /// Register a test user with a unique username to prevent conflicts
        /// </summary>
        public async Task<AuthResponseDto> RegisterTestUserAsync(HttpClient client, string email = null, string password = "Test123!")
        {
            try
            {
                // Generate a unique email if one wasn't provided
                email = email ?? GetUniqueEmail();
                // Skip the API registration and just create a user directly
                using var scope = this.Services.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BourbonVault.Core.Models.ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                
                // Ensure the User role exists
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                    Console.WriteLine("Created 'User' role that was missing");
                }
                
                // Delete the user if it already exists
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    Console.WriteLine($"User {email} already exists. Deleting...");
                    var deleteResult = await userManager.DeleteAsync(existingUser);
                    if (!deleteResult.Succeeded)
                    {
                        Console.WriteLine($"Failed to delete existing user: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
                    }
                }
                
                // Create new test user directly
                var user = new BourbonVault.Core.Models.ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = "Test User",
                    Bio = "Test user bio", 
                    ProfileImageUrl = "https://example.com/profile.jpg"  
                };
                
                Console.WriteLine($"Creating test user {email} directly via UserManager");
                var result = await userManager.CreateAsync(user, password);
                
                if (!result.Succeeded)
                {
                    Console.WriteLine($"UserManager.CreateAsync failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                
                // Add the user to the User role
                var roleResult = await userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine($"Failed to add user to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine("Added user to 'User' role successfully");
                }

                // Generate JWT token for testing
                using var scopeAuth = this.Services.CreateScope();
                var authService = scopeAuth.ServiceProvider.GetRequiredService<BourbonVault.Core.Services.IAuthService>();
                
                // Call login to get token
                var loginResult = await authService.LoginAsync(email, password);
                if (!loginResult.Success)
                {
                    Console.WriteLine($"Failed to generate token: {loginResult.Message}");
                    throw new Exception($"Failed to generate token: {loginResult.Message}");
                }
                
                Console.WriteLine("Test user created and authenticated successfully");
                
                // Return an AuthResponseDto with the token
                return new AuthResponseDto
                {
                    Success = true,
                    Token = loginResult.Token,
                    Username = email,
                    Email = email,
                    DisplayName = "Test User"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RegisterTestUserAsync: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO to match the AuthResponseDto from the API
    /// </summary>
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}
