namespace ChurchDTOs.DTOs.Entities
{
    public class TwoFactorStatusDto
    {
        public bool Enabled { get; set; }
        public string? Type { get; set; }
    }
}