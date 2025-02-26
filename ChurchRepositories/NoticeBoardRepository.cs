using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class NoticeBoardRepository : INoticeBoardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public NoticeBoardRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<NoticeBoardDTO> GetNoticeBoardAsync(
       int parishId,
       DateTime? startDate,
       DateTime? endDate,
       string headName,
       FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
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

            // Retrieve only "Live" families for this parish.
            var families = await _context.Families
                .Where(f => f.ParishId == parishId && f.Status == "Live")
                .ToListAsync();

            List<FinancialReportNoticeBoardDTO> paidMembers = new List<FinancialReportNoticeBoardDTO>();
            List<FinancialReportNoticeBoardDTO> unpaidMembers = new List<FinancialReportNoticeBoardDTO>();

            foreach (var family in families)
            {
               
                var paymentsQuery = _context.FinancialReportsView
                    .Where(tx => tx.ParishId == parishId
                              && tx.FamilyNumber == family.FamilyNumber
                              && tx.HeadName == headName);

                if (startDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(tx => tx.TrDate >= startUtc);
                if (endDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(tx => tx.TrDate <= endUtc);

                // Group by FamilyNumber and summarize payments
                var paymentSummary = await paymentsQuery
                    .GroupBy(tx => tx.FamilyNumber)
                    .Select(g => new
                    {
                        TotalPayment = g.Sum(tx => tx.IncomeAmount),
                        EarliestTrDate = g.Min(tx => tx.TrDate),
                        SampleVrNo = g.Min(tx => tx.VrNo),                        
                        UnitId = g.Min(tx => tx.UnitId),
                        UnitName = g.Min(tx => tx.UnitName),
                        FamilyId= g.Min(tx => tx.FamilyId),
                        FamilyNumber =g.Min(tx => tx.FamilyNumber)
                    })
                    .FirstOrDefaultAsync();

                // Summarize payment info
                decimal totalPayment = paymentSummary != null ? paymentSummary.TotalPayment : 0;
                DateTime trDate = paymentSummary != null ? paymentSummary.EarliestTrDate : DateTime.MinValue;
                string vrNo = paymentSummary != null ? paymentSummary.SampleVrNo : string.Empty;               
                string unitName = paymentSummary != null ? paymentSummary.UnitName : string.Empty;

                // Build a combined family name: head name + family name.
                string combinedFamilyName = $"{family.HeadName} {family.FamilyName}".Trim();

                // Create a "fake" FinancialReportsView object to drive our mapping.
                // (We need to supply the properties that our mapping profile uses.)
                var fakeFinancialReport = new FinancialReportsView
                {
                    TrDate = trDate,
                    VrNo = vrNo,
                    IncomeAmount = totalPayment,                    
                    FamilyId = family.FamilyId,
                    FamilyNumber = family.FamilyNumber,
                    FamilyName = combinedFamilyName,
                    UnitId = family.UnitId,
                    UnitName = unitName,
                    HeadName = headName
                };

                // Use AutoMapper to map the fake object to our NoticeBoard DTO.
                var noticeDTO = _mapper.Map<FinancialReportNoticeBoardDTO>(fakeFinancialReport, opts =>
                {
                    opts.Items["CustomizationOption"] = customizationOption;
                });              

                // Add to the appropriate list.
                if (totalPayment > 0)
                    paidMembers.Add(noticeDTO);
                else
                    unpaidMembers.Add(noticeDTO);
            }

            return new NoticeBoardDTO
            {
                PaidMembers = paidMembers,
                UnpaidMembers = unpaidMembers
            };
        }


    }
}
