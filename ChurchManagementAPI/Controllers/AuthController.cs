using ChurchCommon.Utils;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AESEncryptionHelper _aesEncryptionHelper;

        public AuthController(AuthService authService, AESEncryptionHelper aesEncryptionHelper)
        {
            _authService = authService;
            _aesEncryptionHelper = aesEncryptionHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            string decryptedUsername = loginDto.Username;
            string decryptedPassword = loginDto.Password;

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";

            var result = await _authService.AuthenticateUserAsync(decryptedUsername, decryptedPassword, ipAddress, userAgent);

            if (result is AuthResultDto authResult)
            {
                if (!authResult.IsSuccess)
                {
                    return Unauthorized(new { Message = authResult.Message });
                }

                return Ok(new
                {
                    Token = authResult.Token,
                    Message = authResult.Message,
                    FullName = authResult.FullName,
                    ParishId = authResult.ParishId,
                    ParishName = authResult.ParishName,
                    FamilyId = authResult?.FamilyId,
                    FamilyName = authResult?.FamilyName,
                    Roles = authResult.Roles
                });
            }

            if (result is TwoFactorRequiredResponseDto twoFactorResult)
            {
                return Ok(twoFactorResult);
            }

            return StatusCode(500, new { Message = "An unexpected error occurred." });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _authService.RegisterUserAsync(registerDto);
                if (user == null)
                {
                    return BadRequest(new { Message = "User registration failed." });
                }

                return Ok(new { Message = "User registered successfully.", UserId = user.Id });
            }
            catch (Exception ex)
            {
                // Check for PostgreSQL duplicate key constraint violation
                if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    if (pgEx.ConstraintName == "users_email_key")
                    {
                        return Conflict(new { Message = "A user with this email address already exists." });
                    }
                    else if (pgEx.ConstraintName?.Contains("username") == true)
                    {
                        return Conflict(new { Message = "A user with this username already exists." });
                    }
                    else
                    {
                        return Conflict(new { Message = "A user with these details already exists." });
                    }
                }

                // Check for user creation validation errors
                if (ex.Message.StartsWith("User creation failed:"))
                {
                    return BadRequest(new { Message = ex.Message });
                }

                // Log and return generic error for unexpected exceptions
                return StatusCode(500, new { Message = "An error occurred during registration. Please try again later." });
            }
        }


    }


}
