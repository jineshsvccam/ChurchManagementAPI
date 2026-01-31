using ChurchContracts.Interfaces.Services;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ChurchManagementAPI.Controllers.Auth
{
    [ApiController]
    [Route("[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;

        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        // ==================== EMAIL VERIFICATION ====================

        [Authorize]
        [HttpPost("email/send")]
        [ProducesResponseType(typeof(EmailVerificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationDto request)
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

            var result = await _verificationService.SendEmailVerificationAsync(userId, request.Email);
            return Ok(result);
        }

        [HttpPost("email/verify")]
        [ProducesResponseType(typeof(EmailVerificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var result = await _verificationService.VerifyEmailAsync(request.Token);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // ==================== PHONE VERIFICATION ====================

        [AllowAnonymous]
        [EnableRateLimiting("verification-send")]
        [HttpPost("phone/send")]
        [ProducesResponseType(typeof(PhoneVerificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendPhoneVerification([FromBody] SendPhoneVerificationDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new ErrorResponseDto { Message = "UserId is required." });
            }

            var result = await _verificationService.SendPhoneVerificationAsync(request.UserId, request.PhoneNumber);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("phone/verify")]
        [ProducesResponseType(typeof(PhoneVerificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new ErrorResponseDto { Message = "UserId is required." });
            }

            var result = await _verificationService.VerifyPhoneAsync(request.UserId, request.Otp);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // ==================== PASSWORD RESET ====================

        [HttpPost("password/forgot")]
        [ProducesResponseType(typeof(PasswordResetRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var result = await _verificationService.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }

        [HttpPost("password/reset")]
        [ProducesResponseType(typeof(ResetPasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponseDto { Message = errorMessage });
            }

            var result = await _verificationService.ResetPasswordAsync(request.Token, request.NewPassword);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
