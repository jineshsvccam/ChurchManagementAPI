using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class ContributionSettings
    {
        public int SettingId { get; set; }
        public int HeadId { get; set; }
        public int ParishId { get; set; }
        public decimal Amount { get; set; }
        public string Frequency { get; set; } = string.Empty; // Monthly/Annually
        public int? DueDay { get; set; }
        public int? DueMonth { get; set; }
        public decimal? FineAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public string Category { get; set; } = string.Empty; // Low, Middle, High
    }

}
