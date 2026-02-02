namespace ChurchDTOs.DTOs.Entities
{
    public class TwoFactorRequiredResponseDto
    {
        public bool IsSuccess { get; set; } = true;
        public string AuthStage { get; set; } = "REQUIRES_2FA";
        public string? TempToken { get; set; }
        public bool IsTwoFactorRequired { get; set; } = true;
        public string? TwoFactorType { get; set; }
        public string Message { get; set; } = "Two-factor authentication is required.";

        // Indicates if this is the user's first successful login (before 2FA)
        public bool IsFirstLogin { get; set; } = false;

        // Indicates whether a 2FA mechanism (authenticator) is already configured for the user
        public bool IsTwoFactorConfigured { get; set; } = false;
    }
}
