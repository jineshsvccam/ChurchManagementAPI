namespace ChurchDTOs.DTOs.Entities
{
    public class RecoveryCodeDto
    {
        public string Code { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
