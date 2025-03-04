using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class FinancialYearDto : IParishEntity
    {
        public int FinancialYearId { get; set; }
        public int ParishId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockDate { get; set; }
        public string? Description { get; set; }
    }
}
