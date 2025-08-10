using BourbonVault.Core.Models;
using BourbonVault.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BourbonVault.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Token, string Message)> RegisterUserAsync(
            string username, string email, string password, string displayName)
        {
            try
            {
                // Enhanced debugging
                System.Console.WriteLine($"RegisterUserAsync called with username={username}, email={email}, displayName={displayName}");
                
                if (string.IsNullOrEmpty(username))
                    return (false, null, "Username cannot be empty");
                if (string.IsNullOrEmpty(email))
                    return (false, null, "Email cannot be empty");
                if (string.IsNullOrEmpty(password))
                    return (false, null, "Password cannot be empty");
                if (string.IsNullOrEmpty(displayName))
                    return (false, null, "DisplayName cannot be empty");
                    
                // Check if user exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    System.Console.WriteLine($"User with email {email} already exists");
                    return (false, null, "User with this email already exists");
                }

                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    DisplayName = displayName,
                    // Add default values for required fields
                    ProfileImageUrl = "https://example.com/default-profile.jpg",
                    Bio = "New user"
                };
                
                System.Console.WriteLine("Creating user with properties:");
                System.Console.WriteLine($"- UserName: {user.UserName}");
                System.Console.WriteLine($"- Email: {user.Email}");
                System.Console.WriteLine($"- DisplayName: {user.DisplayName}");
                System.Console.WriteLine($"- ProfileImageUrl: {user.ProfileImageUrl}");
                System.Console.WriteLine($"- Bio: {user.Bio}");

                // Check if password meets requirements
                var passwordValidator = new PasswordValidator<ApplicationUser>();
                var passwordResult = await passwordValidator.ValidateAsync(_userManager, null, password);
                if (!passwordResult.Succeeded)
                {
                    var passwordErrors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                    System.Console.WriteLine($"Password validation failed: {passwordErrors}");
                    return (false, null, $"Invalid password: {passwordErrors}");
                }

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    System.Console.WriteLine($"User creation failed: {errors}");
                    return (false, null, errors);
                }
                
                System.Console.WriteLine($"User {username} created successfully");

               
                    
                // Add user to 'User' role
                try {
                    System.Console.WriteLine("Adding user to 'User' role");
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        System.Console.WriteLine($"Failed to add user to role: {roleErrors}");
                    }
                    else
                    {
                        System.Console.WriteLine("User added to 'User' role successfully");
                    }
                }
                catch (Exception roleEx)
                {
                    System.Console.WriteLine($"Exception adding user to role: {roleEx.Message}");
                }

                // Return token for auto-login after registration
                var tokenResult = await GenerateTokenAsync(user);
                System.Console.WriteLine($"Token generated: {(tokenResult.Success ? "Success" : "Failed")} - {tokenResult.Message}");
                return tokenResult;
            }
            catch (Exception ex)
            {
                // Log the exception details
                System.Console.WriteLine($"Exception in RegisterUserAsync: {ex.Message}");
                System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                return (false, null, $"Server error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Token, string Message)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, null, "User not found");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return (false, null, "Invalid email or password");
            }

            return await GenerateTokenAsync(user);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        private async Task<(bool Success, string Token, string Message)> GenerateTokenAsync(ApplicationUser user)
        {
            try {
                var userRoles = await _userManager.GetRolesAsync(user);
                System.Console.WriteLine($"User roles for {user.Email}: {string.Join(", ", userRoles)}");
    
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("DisplayName", user.DisplayName ?? "")
                };
    
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                
                // Get JWT secret - ensure it's at least 64 bytes for HMACSHA512
                string secretKey;
                // Check if running in test environment
                if (_configuration["IsTestEnvironment"] == "true" || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    // Use a known test key that's at least 64 bytes
                    secretKey = "TestSecretKeyForIntegrationTestsOnly123456789012345678901234567890123456789012345"; 
                    System.Console.WriteLine("Using test environment JWT key");
                }
                else
                {
                    secretKey = _configuration["JwtSettings:Secret"] ?? 
                                "DefaultSecretKeyForDevelopment1234567890123456789012345678901234567890123456789"; // Fallback
                }
                
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                
                // Check key size
                int keySize = key.Key.Length * 8; // Convert bytes to bits
                System.Console.WriteLine($"JWT key size: {keySize} bits");
                
                // Use SHA256 for testing to avoid key size issues
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(7), // Token valid for 7 days
                    SigningCredentials = creds,
                    Issuer = _configuration["JwtSettings:Issuer"] ?? "BourbonVaultAPI",
                    Audience = _configuration["JwtSettings:Audience"] ?? "BourbonVaultClients"
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return (true, tokenHandler.WriteToken(token), "Authentication successful");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Token generation error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return (false, null, $"Authentication error: {ex.Message}");
            }
        }
    }
}
