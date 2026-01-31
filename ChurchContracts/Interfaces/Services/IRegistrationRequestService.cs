using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts.Interfaces.Services
{
    public interface IRegistrationRequestService
    {
        Task<RegisterRequestResponseDto> CreateRegisterRequestAsync(RegisterRequestDto request, string ipAddress);
        Task<VerifyRegistrationEmailResponseDto> VerifyRegistrationEmailAsync(string token);
        Task<RegistrationPhoneVerificationResponseDto> SendRegistrationPhoneOtpAsync(SendRegistrationPhoneOtpDto request, string ipAddress);
        Task<RegistrationPhoneVerificationResponseDto> VerifyRegistrationPhoneOtpAsync(VerifyRegistrationPhoneOtpDto request, string ipAddress);
        Task<CompleteRegistrationResponseDto> CompleteRegistrationAsync(CompleteRegistrationDto request);
    }
}
