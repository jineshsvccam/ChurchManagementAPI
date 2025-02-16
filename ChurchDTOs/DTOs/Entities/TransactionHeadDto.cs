using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class TransactionHeadDto
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; } = null!;
        public string? Type { get; set; }
        public string? Description { get; set; }
    }

}
