using ChurchDTOs.DTOs.Utils;

namespace ChurchContracts
{
    internal interface IReportsAll
    {
    }

    #region Repositories Interfaces
    public interface ILedgerRepository
    {
        Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(
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

    #endregion

    #region Services Interfaces
    public interface ILedgerService
    {
           Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(
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
    #endregion
}
