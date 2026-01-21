using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices.Reports
{
    public class KudishikaReportService : IKudishikaReportService
    {
        private readonly IKudishikaReportRepository _kudishikaReportRepository;

        public KudishikaReportService(IKudishikaReportRepository kudishikaReportRepository)
        {
            _kudishikaReportRepository = kudishikaReportRepository;
        }

        public async Task<KudishikalReportDTO> GetKudishikaReportAsync(int parishId, int familyNumber, DateTime? startDate, DateTime? endDate, bool istransactionrequired = true)
        {
            return await _kudishikaReportRepository.GetKudishikaReportAsync(parishId, familyNumber, startDate, endDate, istransactionrequired);
        }
    }
}
