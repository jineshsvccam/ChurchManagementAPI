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
            var result = await _authService.AuthenticateUserAsync(loginDto.Username, loginDto.Password);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { Message = result.Message });
            }

            return Ok(new
            {
                Token = result.Token,
                Message = result.Message,
                FullName = result.FullName,
                ParishId = result.ParishId,
                ParishName = result.ParishName,
                FamilyId = result.FamilyId,
                FamilyName = result.FamilyName,
                Roles = result.Roles
            });
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
