using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class FamilyDto
    {
        public int FamilyId { get; set; }
        public string? FamilyName { get; set; }
        public int UnitId { get; set; }
        public string? Address { get; set; }
        public string? ContactInfo { get; set; }
        public string? Category { get; set; }
        public int FamilyNumber { get; set; }
        public string? Status { get; set; }
        public string? HeadName { get; set; }
        public DateTime JoinDate { get; set; }
    }

}
