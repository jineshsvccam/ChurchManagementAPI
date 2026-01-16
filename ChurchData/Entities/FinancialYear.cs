using System;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class FinancialYear
    {
        public int FinancialYearId { get; set; }
        public int ParishId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockDate { get; set; }
        public string? Description { get; set; }

       [JsonIgnore]
        public Parish? Parish { get; set; }
    }
}
