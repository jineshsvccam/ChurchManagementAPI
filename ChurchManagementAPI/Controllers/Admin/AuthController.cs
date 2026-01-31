using ChurchContracts;
using ChurchContracts.Interfaces.Services;
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
        private readonly IRegistrationRequestService _registrationRequestService;

        public AuthController(IAuthService authService, IRegistrationRequestService registrationRequestService)
        {
            _authService = authService;
            _registrationRequestService = registrationRequestService;
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

            // Verification required responses are returned as anonymous objects. Return them as-is.
            return Ok(result);
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

            var roleName = "FamilyMember";
            if (registerDto.RoleIds != null && registerDto.RoleIds.Count > 0)
            {
                // Preserve legacy behavior: take first role id and translate to role name for staging.
                var role = await HttpContext.RequestServices
                    .GetRequiredService<ChurchContracts.IRoleService>()
                    .GetRoleByIdAsync(registerDto.RoleIds[0]);

                if (!string.IsNullOrWhiteSpace(role?.Name))
                {
                    roleName = role.Name;
                }
            }

            var request = new RegisterRequestDto
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                ParishId = registerDto.ParishId,
                FamilyId = registerDto.FamilyId,
                Role = roleName
            };

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _registrationRequestService.CreateRegisterRequestAsync(request, ip);

            return Ok(new { result.IsSuccess, result.Message });
        }

        [Authorize]
        [HttpPost("2fa/disable")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequestDto request)
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
                var result = await _authService.DisableTwoFactorAsync(userId, request.Code);
                return Ok(new { isSuccess = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while disabling 2FA." });
            }
        }

        [Authorize]
        [HttpGet("2fa/status")]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new ErrorResponseDto { Message = "User ID not found in token." });
            }

            try
            {
                var status = await _authService.GetTwoFactorStatusAsync(userId);
                return Ok(status);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseDto { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDto { Message = "An error occurred while retrieving 2FA status." });
            }
        }
    }
}
