using BourbonVault.Core.DTOs;
using BourbonVault.Core.Models;
using BourbonVault.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BourbonVault.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RegisterUserAsync(
                    registerDto.Username,
                    registerDto.Email,
                    registerDto.Password,
                    registerDto.DisplayName);

                if (!result.Success)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Token = result.Token,
                    Message = "Registration successful",
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    DisplayName = registerDto.DisplayName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, "An error occurred during registration");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

                if (!result.Success)
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                // Get user details for response
                var userManager = HttpContext.RequestServices.GetService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;
                var user = await userManager.FindByEmailAsync(loginDto.Email);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Token = result.Token,
                    Message = "Login successful",
                    Username = user.UserName,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, "An error occurred during login");
            }
        }
        
        [AllowAnonymous]
        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
        {
            try
            {
                var exists = await _authService.UserExistsAsync(email);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence");
                return StatusCode(500, "An error occurred while checking email");
            }
        }
    }
}
