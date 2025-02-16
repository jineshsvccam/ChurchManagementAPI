using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class BankDto
    {
        public int BankId { get; set; }
        public string BankName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
