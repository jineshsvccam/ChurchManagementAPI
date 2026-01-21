using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchDTOs.DTOs.Utils;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Reports
{
    public class FamilyReportService : IFamilyReportService
    {
        private readonly IFamilyReportRepository _familyReportRepository;
        private readonly ILogger<FamilyReportService> _logger;

        public FamilyReportService(IFamilyReportRepository familyReportRepository, ILogger<FamilyReportService> logger)
        {
            _familyReportRepository = familyReportRepository;
            _logger = logger;
        }

        public async Task<FamilyReportDTO> GetFamilyReportAsync(int parishId, int familyNumber)
        {
            _logger.LogInformation("Generating family report for ParishId: {ParishId}, FamilyNumber: {FamilyNumber}", 
                parishId, familyNumber);
            return await _familyReportRepository.GetFamilyReportAsync(parishId, familyNumber);
        }
    }
}
