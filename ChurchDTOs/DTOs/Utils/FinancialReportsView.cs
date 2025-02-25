using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class FinancialReportsView
    {
        public int TransactionId { get; set; }
        public DateTime TrDate { get; set; }
        public string VrNo { get; set; }
        public string TransactionType { get; set; }
        public int HeadId { get; set; }       
        public int FamilyId { get; set; }      
        public int BankId { get; set; }
        public int ParishId { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string Description { get; set; }

        public string HeadName { get; set; }
        public string FamilyName { get; set; }
        public string BankName { get; set; }
       
        public string ParishName { get; set; }
    }
}
