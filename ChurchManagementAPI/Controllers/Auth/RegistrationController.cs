using ChurchContracts.Interfaces.Services;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChurchManagementAPI.Controllers.Auth
{
    [ApiController]
    [Route("auth")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationRequestService _registrationService;

        public RegistrationController(IRegistrationRequestService registrationService)
        {
            _registrationService = registrationService;
        }

        [EnableRateLimiting("register-request-email")]
        [HttpPost("register-request")]
        [ProducesResponseType(typeof(RegisterRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterRequest([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _registrationService.CreateRegisterRequestAsync(request, ip);
            result.Token = null;
            return Ok(result);
        }

        [HttpPost("verify-registration-email")]
        [ProducesResponseType(typeof(VerifyRegistrationEmailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyRegistrationEmail([FromBody] VerifyRegistrationEmailDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var result = await _registrationService.VerifyRegistrationEmailAsync(request.Token);
            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseDto { Message = result.Message });
            }

            return Ok(result);
        }

        [HttpPost("complete-registration")]
        [ProducesResponseType(typeof(CompleteRegistrationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var result = await _registrationService.CompleteRegistrationAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseDto { Message = result.Message });
            }

            return Ok(result);
        }

        [HttpPost("registration/phone/send-otp")]
        [ProducesResponseType(typeof(RegistrationPhoneVerificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendRegistrationPhoneOtp([FromBody] SendRegistrationPhoneOtpDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _registrationService.SendRegistrationPhoneOtpAsync(request, ip);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseDto { Message = result.Message });
            }

            return Ok(result);
        }

        [HttpPost("registration/phone/verify-otp")]
        [ProducesResponseType(typeof(RegistrationPhoneVerificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyRegistrationPhoneOtp([FromBody] VerifyRegistrationPhoneOtpDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _registrationService.VerifyRegistrationPhoneOtpAsync(request, ip);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponseDto { Message = result.Message });
            }

            return Ok(result);
        }
    }
}
