using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var (isSuccess, token, message, fullName) = await _authService.AuthenticateUserAsync(loginDto.Username, loginDto.Password);

            if (!isSuccess)
            {
                return Unauthorized(new { Message = message });
            }

            return Ok(new { Token = token, Message = message, FullName = fullName });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = await _authService.RegisterUserAsync(registerDto);
            if (user == null)
            {
                return BadRequest("User registration failed.");
            }

            return Ok(new { Message = "User registered successfully.", UserId = user.Id });
        }

      
    }


}
