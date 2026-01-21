using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;

namespace ChurchManagementAPI.Controllers.Admin
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TwoFactorRequiredResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
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
                    return Unauthorized(new ErrorResponseDto { Message = authResult.Message });
                }

                return Ok(authResult);
            }

            if (result is TwoFactorRequiredResponseDto twoFactorResult)
            {
                return Ok(twoFactorResult);
            }

            return StatusCode(500, new ErrorResponseDto { Message = "An unexpected error occurred." });
        }

        [HttpPost("verify-2fa")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";

            var result = await _authService.VerifyTwoFactorAsync(request.TempToken, request.Code, ipAddress, userAgent);

            if (!result.IsSuccess)
            {
                return Unauthorized(new ErrorResponseDto { Message = result.Message });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("2fa/setup")]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new ErrorResponseDto { Message = "User ID not found in token." });
            }

            try
            {
                var result = await _authService.SetupTwoFactorAsync(userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while setting up 2FA." });
            }
        }

        [Authorize]
        [HttpPost("2fa/verify-setup")]
        public async Task<IActionResult> VerifySetupTwoFactor([FromBody] VerifyAuthenticatorRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new ErrorResponseDto { Message = "User ID not found in token." });
            }

            try
            {
                var result = await _authService.VerifySetupTwoFactorAsync(userId, request.Code);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while verifying 2FA setup." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            try
            {
                var user = await _authService.RegisterUserAsync(registerDto);
                if (user == null)
                {
                    return BadRequest(new ErrorResponseDto { Message = "User registration failed." });
                }

                return Ok(new { Message = "User registered successfully.", UserId = user.Id });
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    if (pgEx.ConstraintName == "users_email_key")
                    {
                        return Conflict(new ErrorResponseDto { Message = "A user with this email address already exists." });
                    }
                    else if (pgEx.ConstraintName?.Contains("username") == true)
                    {
                        return Conflict(new ErrorResponseDto { Message = "A user with this username already exists." });
                    }
                    else
                    {
                        return Conflict(new ErrorResponseDto { Message = "A user with these details already exists." });
                    }
                }

                if (ex.Message.StartsWith("User creation failed:"))
                {
                    return BadRequest(new ErrorResponseDto { Message = ex.Message });
                }

                return StatusCode(500, new ErrorResponseDto { Message = "An error occurred during registration. Please try again later." });
            }
        }
    }
}
