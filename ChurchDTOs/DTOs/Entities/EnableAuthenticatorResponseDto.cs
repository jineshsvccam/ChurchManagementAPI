namespace ChurchDTOs.DTOs.Entities
{
    public class EnableAuthenticatorResponseDto
    {
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public List<string> RecoveryCodes { get; set; } = new List<string>();
    }
}
