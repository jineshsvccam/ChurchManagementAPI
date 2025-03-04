using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{

    public class ReportsController : ManagementAuthorizedController<ReportsController>
    {
        private readonly ILedgerService _ledgerService;
        private readonly IBankConsolidatedStatementService _bankService;
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly ICashBookService _cashBookService;
        private readonly INoticeBoardService _noticeBoardService;
        private readonly IAllTransactionsService _allTransactionsService;
        private readonly IAramanaReportService _aramanaReportService;
        private readonly IFamilyReportService _familyReportService;
        private readonly IKudishikaReportService _kudishikaReportService;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILedgerService ledgerService,
            IBankConsolidatedStatementService bankService,
            ITrialBalanceService trialBalanceService,
            ICashBookService cashBookService,
            INoticeBoardService noticeBoardService,
            IAllTransactionsService allTransactionsService,
            IAramanaReportService aramanaReportService,
            IFamilyReportService familyReportService,
            IKudishikaReportService kudishikaReportService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ReportsController> logger) : base(httpContextAccessor, context,logger)

        {
            _ledgerService = ledgerService;
            _bankService = bankService;
            _trialBalanceService = trialBalanceService;
            _cashBookService = cashBookService;
            _noticeBoardService = noticeBoardService;
            _allTransactionsService = allTransactionsService;
            _aramanaReportService = aramanaReportService;
            _familyReportService = familyReportService;
            _kudishikaReportService = kudishikaReportService;
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<LedgerReportDTO>> GetLedger(
            [FromQuery] int parishId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var ledger = await _ledgerService.GetLedgerAsync(parishId, startDate, endDate, includeTransactions, customizationOption);
            return Ok(ledger);
        }

        [HttpGet("bankstatement")]
        public async Task<ActionResult<BankStatementConsolidatedDTO>> GetBankStatement(
            [FromQuery] int parishId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool includeTransactions = false,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var bankStatement = await _bankService.GetBankStatementAsync(parishId, startDate, endDate, includeTransactions, customizationOption);
            return Ok(bankStatement);
        }

        [HttpGet("trialbalance")]
        public async Task<ActionResult<TrialBalanceDTO>> GetTrialBalances(
           [FromQuery] int parishId,
           [FromQuery] DateTime startDate,  // assuming required if includeTransactions true
           [FromQuery] DateTime endDate,
           [FromQuery] bool includeTransactions = false,
           [FromQuery] FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var trialBalance = await _trialBalanceService.GetTrialBalanceAsync(parishId, startDate, endDate, includeTransactions, customizationOption);
            return Ok(trialBalance);
        }
        [HttpGet("cashbook")]
        public async Task<ActionResult<TrialBalanceDTO>> GetCashBook(
              [FromQuery] int parishId,
              [FromQuery] DateTime startDate,
              [FromQuery] DateTime endDate,
              [FromQuery] string bankName = "All",
              [FromQuery] FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var cashbook = await _cashBookService.GetCashBookAsync(parishId, startDate, endDate, bankName, customizationOption);
            return Ok(cashbook);
        }

        [HttpGet("noticeboard")]
        public async Task<ActionResult<NoticeBoardDTO>> GetNoticeBoard(
              [FromQuery] int parishId,
              [FromQuery] DateTime startDate,
              [FromQuery] DateTime endDate,
              [FromQuery] string headName,
              [FromQuery] FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var noticeBoard = await _noticeBoardService.GetNoticeBoardAsync(parishId, startDate, endDate, headName, customizationOption);
            return Ok(noticeBoard);
        }

        [HttpGet("allTransactions")]
        public async Task<ActionResult<AllTransactionReportDTO>> GetAllTransactions(
             [FromQuery] int parishId,
             [FromQuery] DateTime startDate,
             [FromQuery] DateTime endDate,
             [FromQuery] FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
           // await ValidateParishIdAsync(parishId);
            var allTransactionReport = await _allTransactionsService.GetAllTransactionAsync(parishId, startDate, endDate, customizationOption);
            return Ok(allTransactionReport);
        }

        [HttpGet("aramanaReport")]
        public async Task<ActionResult<AramanaReportDTO>> GetAramanaReport(
            [FromQuery] int parishId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var aramanareport = await _aramanaReportService.GetAramanaReportAsync(parishId, startDate, endDate);
            return Ok(aramanareport);
        }

        [HttpGet("familyReport")]
        public async Task<ActionResult<FamilyReportDTO>> GetFamilyReport(
           [FromQuery] int parishId,
           [FromQuery] int familyNumber)
        {
            var familyReport = await _familyReportService.GetFamilyReportAsync(parishId, familyNumber);
            return Ok(familyReport);
        }

        [HttpGet("kudishikaReport")]
        public async Task<ActionResult<FamilyReportDTO>> GetKudishikaReport(
          [FromQuery] int parishId,
          [FromQuery] int familyNumber,
          [FromQuery] DateTime startDate,
          [FromQuery] DateTime endDate)
        {
            var familyReport = await _kudishikaReportService.GetKudishikaReportAsync(parishId, familyNumber, startDate, endDate);
            return Ok(familyReport);
        }
    }
}
