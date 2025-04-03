using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class FamilyContribution
    {
        public int ContributionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int HeadId { get; set; }
        public int FamilyId { get; set; }
        public int BankId { get; set; }
        public int ParishId { get; set; }
        public int SettingId { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string? Description { get; set; }

        // Newly added columns
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ensures UTC time for consistency
        public Guid CreatedBy { get; set; } // UUID (non-nullable)
        public DateTime? UpdatedAt { get; set; } // Nullable, updated only on changes
        public Guid? UpdatedBy { get; set; } // Nullable, updated only on changes
        public string? BillName { get; set; }
    }
}
