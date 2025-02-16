using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class RecurringTransactionDto
    {
        public int HeadId { get; set; }
        public int FamilyId { get; set; }
        public int ParishId { get; set; }
        public decimal IncomeAmount { get; set; }
    }
}
