using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class AramanaReportRepository : IAramanaReportRepository
    {
        private readonly IAllTransactionsRepository _allTransactionRepository;
        private readonly ApplicationDbContext _context;
        public AramanaReportRepository( IAllTransactionsRepository allTransactionRepository, ApplicationDbContext context)
        {
            _allTransactionRepository = allTransactionRepository;
            _context = context;
        }
        public async Task<AramanaReportDTO> GetAramanaReportAsync(
         int parishId,
         DateTime? startDate,
         DateTime? endDate)

        {
            // 1. Retrieve all transactions using the AllTransaction repository.
            // (Assuming _allTransactionRepository is injected and returns AllTransactionReportDTO)
            var allTransReport = await _allTransactionRepository.GetAllTransactionAsync(parishId, startDate, endDate, FinancialReportCustomizationOption.Both);
            var allTransactions = allTransReport.Transactions; // List<FinancialReportCustomDTO>

            // 2. Get all TransactionHeads where Description contains 'Aramana'
            var aramanaHeads = await _context.TransactionHeads
                                             .Where(th => th.ParishId == parishId &&
                                                          ((th.Description != null && th.Description.Contains("Aramana"))
                                                           || th.HeadName == "Aramana"))
                                             .ToListAsync();

            List<AramanaDetails> detailsList = new List<AramanaDetails>();

            // 3. Loop through each transaction head.
            foreach (var head in aramanaHeads)
            {
                // Filter transactions that match this head.
                // (We match on HeadName here; adjust if needed.)
                var headTransactions = allTransactions.Where(tx => tx.HeadName == head.HeadName).ToList();

                // Sum the amounts.
                decimal totalIncome = headTransactions.Sum(tx => tx.IncomeAmount);
                decimal totalExpense = headTransactions.Sum(tx => tx.ExpenseAmount);

                // 4. Multiply each sum with the head's Aramanapct value.
                // If Aramanapct is null, we treat it as 0.
                decimal pct = head.Aramanapct.HasValue ? Convert.ToDecimal(head.Aramanapct.Value) : 0m;
                decimal paid = totalIncome * pct / 100;
                decimal toBePaid = totalExpense * pct / 100;

                // Create a DTO for this head.
                AramanaDetails detail = new AramanaDetails
                {
                    HeadName = head.HeadName,
                    Paid = toBePaid,
                    ToBePaid = paid,
                    Balance = toBePaid - paid
                };

                detailsList.Add(detail);
            }

            // 5. Package the details into the report DTO.
            AramanaReportDTO report = new AramanaReportDTO
            {
                AramanaDetails = detailsList
            };

            return report;
        }

    }
}
