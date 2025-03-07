using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
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
        public List<FinancialReportCustomDTO> Transactions { get; set; } = new List<FinancialReportCustomDTO>();
    }
    public class BankDTO
    {

        public int? BankId { get; set; }
        public string BankName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Balance { get; set; }
    }

    public class TrialBalanceDTO
    {
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<BankDTO> OpeningDetails { get; set; } = new List<BankDTO>();
        public List<BankDTO> ClosingDetails { get; set; } = new List<BankDTO>();
        public List<HeadDTO> Heads { get; set; }
        public List<FinancialReportCustomDTO> Transactions { get; set; } = new List<FinancialReportCustomDTO>();

    }

    public class CashBookReportDTO
    {
        public List<CashBookDetailDTO> CashBooks { get; set; }
    }
    public class CashBookDetailDTO
    {
        public string BankName { get; set; }
        public double OpeningBalance { get; set; }
        public double ClosingBalance { get; set; }
        public List<FinancialReportCustomDTO> Statements { get; set; }
    }

    public class NoticeBoardDTO
    {
        public List<FinancialReportNoticeBoardDTO> PaidMembers { get; set; }
        public List<FinancialReportNoticeBoardDTO> UnpaidMembers { get; set; }
    }

    public class AllTransactionReportDTO
    {
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }
        public List<FinancialReportCustomDTO> Transactions { get; set; }
    }

    public class AramanaReportDTO
    {
        public List<AramanaDetails> AramanaDetails { get; set; }
    }
    public class AramanaDetails
    {
        public string HeadName { get; set; }
        public decimal ToBePaid { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class FamilyReportDTO
    {
        public int FamilyNumber { get; set; }
        public string FamilyName { get; set; }
        public int FamilyId { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalReceived { get; set; }
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }
        public List<FinancialReportCustomDTO> Transactions { get; set; }

    }

    public class KudishikalReportDTO
    {      
        public int FamilyNumber { get; set; }
        public string FamilyName { get; set; }
        public int FamilyId { get; set; }
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }
        public List<KudishikaDetails> KudishikaItems { get; set; }

    }
    public class KudishikaDetails
    {
        public string HeadName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDues { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<FinancialReportCustomDTO> Transactions { get; set; }
    }

    public class PivotReportDTO
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; }
        public List<MonthAmountDTO> MonthlyAmounts { get; set; } = new List<MonthAmountDTO>();
    }

    public class MonthAmountDTO
    {
        public string MonthLabel { get; set; }   // e.g., "Apr", "May", ... "Mar", "Total"
        public decimal Value { get; set; }
    }

    public class SingleHeadFiscalReportDTO
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; }
        public List<FiscalYearDetail> FiscalYears { get; set; } = new List<FiscalYearDetail>();
    }

    public class FiscalYearDetail
    {
        public int FiscalYear { get; set; }
        public List<MonthValue> MonthValues { get; set; } = new List<MonthValue>();
        public decimal Total { get; set; }
    }

    public class MonthValue
    {
        public string MonthLabel { get; set; } // e.g., "Apr", "May", "Jun", ...
        public decimal Value { get; set; }
    }

    public class FinancialReportNoticeBoardDTO
    {
        public DateTime TrDate { get; set; }
        public string VrNo { get; set; }
        public decimal IncomeAmount { get; set; }

        // For Family
        public int? FamilyId { get; set; }
        public int FamilyNumber { get; set; }
        public string FamilyName { get; set; }

        // For Unit
        public int? UnitId { get; set; }
        public string UnitName { get; set; }

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
        public string? BillName { get; set; }

        // For Head
        public int? HeadId { get; set; }
        public string HeadName { get; set; }

        // For Family
        public int? FamilyId { get; set; }
        public string FamilyName { get; set; }
        public int? FamilyNumber { get; set; }

        // For Bank
        public int? BankId { get; set; }
        public string BankName { get; set; }

        // For Parish
        //  public int? ParishId { get; set; }
        //  public string ParishName { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FinancialReportCustomizationOption
    {
        IdsOnly,    // Only IDs will be included (names will be null)
        NamesOnly,  // Only names will be included (IDs will be null)
        Both        // Both IDs and names will be included
    }
}
