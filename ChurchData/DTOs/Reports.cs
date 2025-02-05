using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData.DTOs
{
    public class LedgerReportDTO
    {
        public List<HeadDTO> Heads { get; set; }
        public List<FinancialReportsView> Transactions { get; set; }
    }
    public class HeadDTO
    {
        public string HeadName { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal Balance { get; set; }
    }

    public class BankStatementConsolidatedDTO
    {
        public List<BankDTO> Banks { get; set; } = new List<BankDTO>();
        public List<FinancialReportsView> Transactions { get; set; } = new List<FinancialReportsView>();
    }

    public class BankDTO
    {
        public string BankName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Balance { get; set; }
    }

    public class TrialBalanceDTO
    {
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<FinancialReportsView> LedgerStatements { get; set; }
    }

    public class BankStatementDTO
    {
        // this can be used for cashbook or bank statement. If cashbook, ItemName will be "Cash"; if bank statement, ItemName will be Bank.
        public string ItemName { get; set; } // either "Cash" or other bank names      
        public List<FinancialReportsView> LedgerStatements { get; set; }
    }
}
