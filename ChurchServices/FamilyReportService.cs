using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class FamilyReportService : IFamilyReportService
    {
        private readonly IFamilyReportRepository _familyReportRepository;
        public FamilyReportService(IFamilyReportRepository familyReportRepository)
        {
            _familyReportRepository = familyReportRepository;
        }
        public async Task<FamilyReportDTO> GetFamilyReportAsync(int parishId, int familyNumber)
        {
            return await _familyReportRepository.GetFamilyReportAsync(parishId, familyNumber);
        }
    }
}
