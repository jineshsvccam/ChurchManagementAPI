using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData.DTOs;

namespace ChurchContracts
{
    internal interface IReportsAll
    {
    }

    #region Repositories Interfaces
    public interface ILedgerRepository
    {
        Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false);
    }
    public interface IBankConsolidatedStatementRepository
    {
        Task<BankStatementConsolidatedDTO> GetBankStatementAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false);
    }

    #endregion

    #region Services Interfaces
    public interface ILedgerService
    {
        Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false);
    }
    public interface IBankConsolidatedStatementService
    {
        Task<BankStatementConsolidatedDTO> GetBankStatementAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false);
    }
    #endregion
}
