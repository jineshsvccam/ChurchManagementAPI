using ChurchDTOs.DTOs.Utils;

namespace ChurchContracts
{
    internal interface IReportsAll
    {
    }

    #region Repositories Interfaces
    public interface ILedgerRepository
    {
        Task<LedgerReportDTO> GetLedgerAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }

    public interface IBankConsolidatedStatementRepository
    {
        Task<BankStatementConsolidatedDTO> GetBankStatementAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface ITrialBalancetRepository
    {
        Task<TrialBalanceDTO> GetTrialBalanceAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface ICashBookRepository
    {
        Task<CashBookReportDTO> GetCashBookAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            string bankName = "All",
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface INoticeBoardRepository
    {
        Task<NoticeBoardDTO> GetNoticeBoardAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            string headName,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface IAllTransactionsRepository
    {
        Task<AllTransactionReportDTO> GetAllTransactionAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
        Task<FinancialReportSummaryDTO> GetAllTransactionGroupedAsync(
                int parishId,
                DateTime? startDate,
                DateTime? endDate,
                FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);

    }
    public interface IAramanaReportRepository
    {
        Task<AramanaReportDTO> GetAramanaReportAsync(
           int parishId,
           DateTime? startDate,
           DateTime? endDate);
    }
    public interface IFamilyReportRepository
    {
        Task<FamilyReportDTO> GetFamilyReportAsync(
            int parishId,
            int familyNumber);
    }

    public interface IKudishikaReportRepository
    {
        Task<KudishikalReportDTO> GetKudishikaReportAsync(
            int parishId,
            int familyNumber,
            DateTime? startDate,
            DateTime? endDate);
    }

    public interface IPivotReportRepository
    {
        Task<PivotReportResult> GetPivotReportAsync(
            int parishId,
            int financialYear,
            string type, // "Income" or "Expense"
             int[]? headIds = null,
             int? headCount = null);
    }
    public interface ISingleHeadFiscalReportRepository
    {
        Task<SingleHeadFiscalReportDto> GetSingleHeadFiscalReportAsync(
            int parishId,
            int headId,
            string type, // "Income" or "Expense"
            int startFiscalYear,
            int endFiscalYear);
    }
    public interface IMonthlyFiscalReportRepository
    {
        Task<MonthlyFiscalReportResponse> GetMonthlyFiscalReportAsync(
            int parishId,
            int startFiscalYear,
            int endFiscalYear);
    }

    #endregion

    #region Services Interfaces
    public interface ILedgerService
    {
        Task<LedgerReportDTO> GetLedgerAsync(
         int parishId,
         DateTime? startDate,
         DateTime? endDate,
         bool includeTransactions = false,
         FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface IBankConsolidatedStatementService
    {
        Task<BankStatementConsolidatedDTO> GetBankStatementAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface ITrialBalanceService
    {
        Task<TrialBalanceDTO> GetTrialBalanceAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface ICashBookService
    {
        Task<CashBookReportDTO> GetCashBookAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            string bankName = "All",
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface INoticeBoardService
    {
        Task<NoticeBoardDTO> GetNoticeBoardAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            string headName,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);
    }
    public interface IAllTransactionsService
    {
        Task<AllTransactionReportDTO> GetAllTransactionAsync(
                   int parishId,
                   DateTime? startDate,
                   DateTime? endDate,
                   FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);

        Task<FinancialReportSummaryDTO> GetAllTransactionGroupedAsync(
                  int parishId,
                  DateTime? startDate,
                  DateTime? endDate,
                  FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both);


    }
    public interface IAramanaReportService
    {
        Task<AramanaReportDTO> GetAramanaReportAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate);
    }
    public interface IFamilyReportService
    {
        Task<FamilyReportDTO> GetFamilyReportAsync(
            int parishId,
            int familyNumber);
    }
    public interface IKudishikaReportService
    {
        Task<KudishikalReportDTO> GetKudishikaReportAsync(
            int parishId,
            int familyNumber,
            DateTime? startDate,
            DateTime? endDate);
    }
    public interface IPivotReportService
    {
        Task<PivotReportResult> GetPivotReportAsync(
            int parishId,
            int financialYear,
            string type,
             int[]? headIds = null,
             int? headCount = null);
    }
    public interface ISingleHeadFiscalReportService
    {
        Task<SingleHeadFiscalReportDto> GetSingleHeadFiscalReportAsync(
            int parishId,
            int headId,
            string type,
            int startFiscalYear,
            int endFiscalYear);
    }
    public interface IMonthlyFiscalReportService
    {
        Task<MonthlyFiscalReportResponse> GetMonthlyFiscalReportAsync(
            int parishId,
            int startFiscalYear,
            int endFiscalYear);
    }
    #endregion
}
