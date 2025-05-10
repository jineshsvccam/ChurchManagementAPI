using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class CashBookRepository : ICashBookRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CashBookRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CashBookReportDTO> GetCashBookAsync(
               int parishId,
               DateTime? startDate,
               DateTime? endDate,
               string bankName = "All",
               FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            List<CashBookDetailDTO> cashBookDetails = new List<CashBookDetailDTO>();

            // Normalize dates to UTC if provided.
            DateTime? startUtc = startDate.HasValue
                ? (startDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                    : startDate.Value)
                : (DateTime?)null;
            DateTime? endUtc = endDate.HasValue
                ? (endDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                    : endDate.Value)
                : (DateTime?)null;

            if (!string.Equals(bankName, "All", StringComparison.OrdinalIgnoreCase))
            {
                // Process a specific bank.
                var detail = await BuildCashBookDetailAsync(parishId, startUtc, endUtc, bankName, customizationOption);
                cashBookDetails.Add(detail);
            }
            else
            {
                // Process all banks for the parish.
                var banks = await _context.Banks
                    .Where(b => b.ParishId == parishId)
                    .Select(b => b.BankName)
                    .ToListAsync();

                foreach (var currentBankName in banks)
                {
                    var detail = await BuildCashBookDetailAsync(parishId, startUtc, endUtc, currentBankName, customizationOption);
                    cashBookDetails.Add(detail);
                }
            }

            return new CashBookReportDTO { CashBooks = cashBookDetails };
        }



        // Separate helper function to build a CashBookDetailDTO for a given bank.
        private async Task<CashBookDetailDTO> BuildCashBookDetailAsync(
            int parishId,
            DateTime? startUtc,
            DateTime? endUtc,
            string currentBankName,
            FinancialReportCustomizationOption customizationOption)
        {
            // Retrieve transactions for the bank within the date range.
            var txQuery = _context.FinancialReportsView
                .Where(r => r.ParishId == parishId && r.BankName == currentBankName)
                .AsQueryable();

            if (startUtc.HasValue)
                txQuery = txQuery.Where(r => r.TrDate >= startUtc.Value);
            if (endUtc.HasValue)
                txQuery = txQuery.Where(r => r.TrDate <= endUtc.Value);

            var transactions = await txQuery.ToListAsync();

            // Map transactions to custom DTO using AutoMapper and the customization option.
            var mappedTransactions = _mapper.Map<List<FinancialReportCustomDTO>>(
                transactions, opts => opts.Items["CustomizationOption"] = customizationOption);

            // Get the bank record from the Banks table.
            var bank = await _context.Banks
                .FirstOrDefaultAsync(b => b.ParishId == parishId && b.BankName == currentBankName);
            if (bank == null)
            {
                throw new Exception($"Bank '{currentBankName}' not found for parish {parishId}.");
            }

            // Calculate the additional amount from transactions before the start date for opening balance.
            double additionalOpening = 0;
            if (startUtc.HasValue)
            {
                additionalOpening = await _context.FinancialReportsView
                    .Where(r => r.ParishId == parishId && r.BankName == currentBankName && r.TrDate < startUtc.Value)
                    .SumAsync(r => (double?)(r.IncomeAmount - r.ExpenseAmount)) ?? 0;
            }
            double openingBalance = Convert.ToDouble(bank.OpeningBalance) + additionalOpening;

            // Calculate
            // the additional amount from transactions up to the end date for closing balance.
            double additionalClosing = 0;
            if (endUtc.HasValue)
            {
                additionalClosing = await _context.FinancialReportsView
                    .Where(r => r.ParishId == parishId && r.BankName == currentBankName && r.TrDate <= endUtc.Value)
                    .SumAsync(r => (double?)(r.IncomeAmount - r.ExpenseAmount)) ?? 0;
            }
            double closingBalance = Convert.ToDouble(bank.OpeningBalance) + additionalClosing;

            // Build the ordered transactions with Odr and RunningBalance
            var orderedTransactions = new List<BankFinancialReportCustomDTO>();
            int order = 1;
            decimal runningBalance = (decimal)openingBalance;

            // Add Opening Balance row explicitly
            orderedTransactions.Add(new BankFinancialReportCustomDTO
            {
                TransactionId = 0,
                TrDate = startUtc ?? DateTime.MinValue,
                VrNo = "OB",
                TransactionType = "Opening Balance",
                IncomeAmount = 0,
                ExpenseAmount = 0,
                Description = "Opening Balance",
                BillName = null,
                HeadId = null,
                HeadName = null,
                FamilyId = null,
                FamilyName = null,
                FamilyNumber = null,
                BankId = bank.BankId,
                BankName = bank.BankName,
                Odr = order++,
                RunningBalance = runningBalance
            });

            // Sort the mapped transactions by date (and maybe by ID if needed for exact order)
            foreach (var t in mappedTransactions.OrderBy(t => t.TrDate).ThenBy(t => t.TransactionId))
            {
                var item = new BankFinancialReportCustomDTO
                {
                    TransactionId = t.TransactionId,
                    TrDate = t.TrDate,
                    VrNo = t.VrNo,
                    TransactionType = t.TransactionType,
                    IncomeAmount = t.IncomeAmount,
                    ExpenseAmount = t.ExpenseAmount,
                    Description = t.Description,
                    BillName = t.BillName,
                    HeadId = t.HeadId,
                    HeadName = t.HeadName,
                    FamilyId = t.FamilyId,
                    FamilyName = t.FamilyName,
                    FamilyNumber = t.FamilyNumber,
                    BankId = t.BankId,
                    BankName = t.BankName,
                    Odr = order++,
                    RunningBalance = runningBalance + t.IncomeAmount - t.ExpenseAmount
                };

                runningBalance = item.RunningBalance;
                orderedTransactions.Add(item);
            }

            return new CashBookDetailDTO
            {
                BankName = currentBankName,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                Statements = orderedTransactions
            };
        }

    }
}
