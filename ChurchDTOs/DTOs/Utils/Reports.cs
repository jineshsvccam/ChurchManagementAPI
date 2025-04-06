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
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }

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
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }
        public decimal CurrentBalance { get; set; }
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
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }

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
    public class AllTransactionReportGroupedDTO
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

    public class FinancialReportSummaryDTO
    {
        public int ParishId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportName { get; set; }

        public List<VRGroupedDTO> Income { get; set; } = new List<VRGroupedDTO>();
        public List<VRGroupedDTO> Expense { get; set; } = new List<VRGroupedDTO>();
        public List<VRGroupedDTO> BankTransfers { get; set; } = new List<VRGroupedDTO>();
        public List<VRGroupedDTO> BulkEntry { get; set; } = new List<VRGroupedDTO>();
    }

    public class VRGroupedDTO
    {
        public string VrNo { get; set; }
        public DateTime Date { get; set; }
        public string? BillName { get; set; }
        public int? FamilyNumber { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public List<FinancialReportCustomDTO> Details { get; set; } = new List<FinancialReportCustomDTO>();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FinancialReportCustomizationOption
    {
        IdsOnly,    // Only IDs will be included (names will be null)
        NamesOnly,  // Only names will be included (IDs will be null)
        Both        // Both IDs and names will be included
    }
    public class PivotReportDto
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; }      

        // Index 0 = Apr, 1 = May, ..., 11 = Mar
        public decimal[] MonthlyAmounts { get; set; } = new decimal[12];

        public decimal Total { get; set; }
    }
    public class PivotReportResult
    {
        public int ParishId { get; set; }
        public int FiscalYear { get; set; }
        public string Type { get; set; } = string.Empty; 
        public int[] HeadIds { get; set; } = Array.Empty<int>();
        public string ReportName { get; set; } = string.Empty;

        public List<PivotReportDto> Data { get; set; } = new();
    }

    public class FiscalYearData
    {
        public int Year { get; set; }
        public decimal[] MonthlyAmounts { get; set; } = new decimal[12];
        public decimal Total { get; set; }
    }

    public class SingleHeadFiscalReportDto
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; }
        public string Type { get; set; }
        public int ParishId { get; set; }      
        public string ReportName { get; set; }

        public List<FiscalYearData> FiscalYears { get; set; } = new();
    }

    public class MonthlyFiscalReportResponse
    {
        public int ParishId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public string ReportName { get; set; }
        public List<FiscalYearIncomeExpenseData> FiscalYears { get; set; } = new();
    }
    public class FiscalYearIncomeExpenseData
    {
        public int Year { get; set; }

        // Index 0 = Apr, 11 = Mar
        public MonthlyIncomeExpense[] MonthlyData { get; set; } = new MonthlyIncomeExpense[12];

        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
    }
    public class MonthlyIncomeExpense
    {
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
   
    public class RawMonthlyFiscalReport
    {
        public int Fyear { get; set; }

        public decimal Apr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Aug { get; set; }
        public decimal Sep { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dec { get; set; }
        public decimal Jan { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }

        public decimal Total { get; set; }

    }
    public class RawPivotReport : RawMonthlyFiscalReport
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; }
    }
}
