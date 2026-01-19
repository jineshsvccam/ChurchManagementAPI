namespace ChurchDTOs.DTOs.Entities
{
    public class TwoFactorRequiredResponseDto
    {
        public string TempToken { get; set; } = string.Empty;
        public string TwoFactorType { get; set; } = string.Empty;
        public string Message { get; set; } = "Two-factor authentication is required.";
    }
}
