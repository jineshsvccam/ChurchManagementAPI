using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChurchData;


namespace ChurchDTOs.DTOs.Utils
{
    public class LedgerReportDTO
    {
        public List<HeadDTO> Heads { get; set; }
        public List<FinancialReportCustomDTO> Transactions { get; set; }
    }
    public class HeadDTO
    {
        public int? HeadId { get; set; }
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

    public class FinancialReportCustomDTO
    {
        public int TransactionId { get; set; }
        public DateTime TrDate { get; set; }
        public string VrNo { get; set; }
        public string TransactionType { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string Description { get; set; }

        // For Head
        public int? HeadId { get; set; }
        public string HeadName { get; set; }

        // For Family
        public int? FamilyId { get; set; }
        public string FamilyName { get; set; }

        // For Bank
        public int? BankId { get; set; }
        public string BankName { get; set; }

        // For Parish
        public int? ParishId { get; set; }
        public string ParishName { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FinancialReportCustomizationOption
    {
        IdsOnly,    // Only IDs will be included (names will be null)
        NamesOnly,  // Only names will be included (IDs will be null)
        Both        // Both IDs and names will be included
    }
}
