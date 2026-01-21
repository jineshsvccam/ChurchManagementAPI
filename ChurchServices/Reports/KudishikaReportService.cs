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
    public class KudishikaReportService : IKudishikaReportService
    {
        private readonly IKudishikaReportRepository _kudishikaReportRepository;
        private readonly ILogger<KudishikaReportService> _logger;

        public KudishikaReportService(IKudishikaReportRepository kudishikaReportRepository, ILogger<KudishikaReportService> logger)
        {
            _kudishikaReportRepository = kudishikaReportRepository;
            _logger = logger;
        }

        public async Task<KudishikalReportDTO> GetKudishikaReportAsync(int parishId, int familyNumber, DateTime? startDate, DateTime? endDate, bool istransactionrequired = true)
        {
            _logger.LogInformation("Generating kudishika report for ParishId: {ParishId}, FamilyNumber: {FamilyNumber}, StartDate: {StartDate}, EndDate: {EndDate}", 
                parishId, familyNumber, startDate, endDate);
            return await _kudishikaReportRepository.GetKudishikaReportAsync(parishId, familyNumber, startDate, endDate, istransactionrequired);
        }
    }
}
