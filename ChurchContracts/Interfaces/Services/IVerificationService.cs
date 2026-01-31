using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts.Interfaces.Services
{
    public interface IVerificationService
    {
        // Email Verification
        Task<EmailVerificationResponseDto> SendEmailVerificationAsync(Guid userId, string email);
        Task<EmailVerificationResponseDto> VerifyEmailAsync(string token);
        
        // Phone Verification
        Task<PhoneVerificationResponseDto> SendPhoneVerificationAsync(Guid userId, string phoneNumber);
        Task<PhoneVerificationResponseDto> VerifyPhoneAsync(Guid userId, string otp);
        
        // Password Reset
        Task<PasswordResetRequestResponseDto> ForgotPasswordAsync(string email);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(string token, string newPassword);
    }
}
