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
    public class KudishikaReportRepository : IKudishikaReportRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public KudishikaReportRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<KudishikalReportDTO> GetKudishikaReportAsync(
             int parishId,
             int familyNumber,
             DateTime? startDate,
             DateTime? endDate)
        {

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

            // 1. Retrieve eligible Kudishika heads from TransactionHeads.
            var kudishikaHeads = await _context.TransactionHeads
                .Where(th => th.ParishId == parishId &&
                             th.Description != null &&
                             th.Description.Contains("Kudishika"))
                .ToListAsync();

            // 2. Retrieve transactions (payments) from FinancialReportsView for this family.
            var payments = await _context.FinancialReportsView
                .Where(r => r.ParishId == parishId && r.FamilyNumber == familyNumber)
                .ToListAsync();

            // 3. Retrieve dues from FinancialReportsViewDues for this family.
            var dues = await _context.FinancialReportsViewDues
                .Where(r => r.ParishId == parishId && r.FamilyNumber == familyNumber)
                .ToListAsync();

            var firstitem = dues.FirstOrDefault();

            // 4. Retrieve opening balances from FamilyDues for this family.
            var familyDues = await _context.FamilyDues
                .Where(fd => fd.ParishId == parishId && fd.FamilyId == firstitem.FamilyId)
                .ToListAsync();

            var openingBalanceDict = familyDues.ToDictionary(fd => fd.HeadId, fd => fd.OpeningBalance);

            List<KudishikaDetails> kudishikaDetailsList = new List<KudishikaDetails>();

            foreach (var head in kudishikaHeads)
            {
                // Calculate the initial opening balance from FamilyDues (if exists).
                decimal initialBalance = openingBalanceDict.ContainsKey(head.HeadId) ? openingBalanceDict[head.HeadId] : 0m;

                // Calculate additional dues and payments BEFORE the start date.
                decimal additionalDues = 0m;
                decimal additionalPayments = 0m;
                if (startDate.HasValue)
                {
                    additionalDues = await _context.FinancialReportsViewDues
                        .Where(d => d.ParishId == parishId
                                 && d.FamilyNumber == familyNumber
                                 && d.HeadId == head.HeadId
                                 && d.TrDate < startUtc.Value)
                        .SumAsync(d => (decimal?)d.ExpenseAmount) ?? 0m;

                    additionalPayments = await _context.FinancialReportsView
                        .Where(p => p.ParishId == parishId
                                 && p.FamilyNumber  == familyNumber
                                 && p.HeadId == head.HeadId
                                 && p.TrDate < startUtc.Value)
                        .SumAsync(p => (decimal?)p.IncomeAmount) ?? 0m;
                }
                // Opening Balance = initial balance + dues (before period) – payments (before period)
                decimal computedOpeningBalance = initialBalance + additionalDues - additionalPayments;

                // For the report period, sum payments and dues.
                decimal totalPaid = payments
                    .Where(p => p.HeadId == head.HeadId
                             && ( p.TrDate >= startUtc.Value)
                             && ( p.TrDate <= endUtc.Value))
                    .Sum(p => p.IncomeAmount);

                decimal totalDue = dues
                    .Where(d => d.HeadId == head.HeadId
                             && ( d.TrDate >= startUtc.Value)
                             && ( d.TrDate <= endUtc.Value))
                    .Sum(d => d.ExpenseAmount);

                // 5. Merge transactions from payments and dues (for the report period) and order by date.
                var periodPayments = payments
                    .Where(p => p.HeadId == head.HeadId
                             && ( p.TrDate >= startUtc.Value)
                             && ( p.TrDate <= endUtc.Value));

                var periodDues = dues
                    .Where(d => d.HeadId == head.HeadId
                             && ( d.TrDate >= startUtc.Value)
                             && ( d.TrDate <= endUtc.Value));

                // 6. Map the combined transactions using AutoMapper.

                // Map each list separately.
                var mappedPayments = _mapper.Map<List<FinancialReportCustomDTO>>(
                    periodPayments,
                    opts => opts.Items["CustomizationOption"] = FinancialReportCustomizationOption.Both
                );

                var mappedDues = _mapper.Map<List<FinancialReportCustomDTO>>(
                    periodDues,
                    opts => opts.Items["CustomizationOption"] = FinancialReportCustomizationOption.Both
                );

                // Now combine them into a single list of FinancialReportCustomDTO
                var combinedTransactions = mappedPayments
                    .Concat(mappedDues)
                    .OrderBy(t => t.TrDate)  // Now you can order by the DTO's TrDate
                    .ToList();

                // Create the Kudishika details DTO for this head.
                var kudishikaDetail = new KudishikaDetails
                {
                    HeadName = head.HeadName,
                    OpeningBalance = computedOpeningBalance,
                    TotalPaid = totalPaid,
                    TotalDues = totalDue,
                    ClosingBalance = computedOpeningBalance + totalDue - totalPaid,
                    Transactions = combinedTransactions
                };

                kudishikaDetailsList.Add(kudishikaDetail);
            }

            return new KudishikalReportDTO
            {
                FamilyId = firstitem.FamilyId,
                FamilyNumber = familyNumber,
                FamilyName = firstitem.FamilyName,
                KudishikaItems = kudishikaDetailsList
            };
        }


    }
}
