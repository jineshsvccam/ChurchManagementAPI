using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData.DTOs
{
    public class ContributionSettingsDto
    {
        public int HeadId { get; set; }
        public int ParishId { get; set; }
        public decimal Amount { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public int? DueDay { get; set; }
        public int? DueMonth { get; set; }
        public decimal? FineAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public string Category { get; set; } = string.Empty;
    }

}
