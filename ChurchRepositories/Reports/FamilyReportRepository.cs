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

namespace ChurchRepositories.Reports
{
    public class FamilyReportRepository : IFamilyReportRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FamilyReportRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FamilyReportDTO> GetFamilyReportAsync(int parishId, int familyNumber)
        {
            // Query the FinancialReportsView for the given parish and family number.
            var familyReportList = await _context.FinancialReportsView
                                 .Where(r => r.ParishId == parishId && r.FamilyNumber == familyNumber)
                                 .OrderBy(r => r.TrDate) // Orders by TrDate in ascending order
                                 .ToListAsync();

            // If no transactions are found, return null.
            if (familyReportList == null || familyReportList.Count == 0)
            {
                return null;
            }

            // Use AutoMapper to map the list of transactions into FinancialReportCustomDTO.
            var mappedTransactions = _mapper.Map<List<FinancialReportCustomDTO>>(
                familyReportList, opts => opts.Items["CustomizationOption"] = FinancialReportCustomizationOption.Both);

            // Sum the IncomeAmount and ExpenseAmount from the view records.
            decimal totalPaid = familyReportList.Sum(x => x.IncomeAmount);
            decimal totalReceived = familyReportList.Sum(x => x.ExpenseAmount);


            // Get family details from the first entry.
            var firstEntry = familyReportList.First();

            var familyReportDTO = new FamilyReportDTO
            {
                FamilyNumber = firstEntry.FamilyNumber,
                FamilyName = firstEntry.FamilyName,
                FamilyId = firstEntry.FamilyId,
                TotalPaid = totalPaid,
                TotalReceived = totalReceived,
                ParishId = parishId,
                StartDate = firstEntry.TrDate,
                EndDate = familyReportList.Last().TrDate,
                ReportName = "Family Report",
                Transactions = mappedTransactions
            };

            return familyReportDTO;
        }

    }
}
