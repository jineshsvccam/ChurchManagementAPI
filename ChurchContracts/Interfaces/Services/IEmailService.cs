namespace ChurchContracts.Interfaces.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task<bool> SendEmailVerificationAsync(string to, string verificationLink);
        Task<bool> SendPasswordResetAsync(string to, string resetLink);
        Task<bool> SendPhoneVerificationAsync(string to, string otp);
    }
}
