using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class FinancialYearDto
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
