using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData.DTOs
{
    public class FamilyDueDto
    {
        public int FamilyId { get; set; }
        public int HeadId { get; set; }
        public int ParishId { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
