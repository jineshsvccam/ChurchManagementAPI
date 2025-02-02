using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class TransactionHead
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string Action { get; set; } // INSERT or UPDATE
        public int HeadId { get; set; }
        public string HeadName { get; set; }
        public string Type { get; set; }
        public bool IsMandatory { get; set; }
        public string Description { get; set; }
        public int ParishId { get; set; }
        public double Aramanapct { get; set; }
        public string Ordr { get; set; }
        public string HeadNameMl { get; set; }

        public Parish? Parish { get; set; }
    }
}
