using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    [Table("family_dues")]
    public class FamilyDue
    {
        [Key]
        [Column("dues_id")]
        public int DuesId { get; set; }

        [Column("family_id")]
        public int FamilyId { get; set; }

        [Column("head_id")]
        public int HeadId { get; set; }

        [Column("parish_id")]
        public int ParishId { get; set; }

        [Column("opening_balance")]
        public decimal OpeningBalance { get; set; }

        [Column("current_balance")]
        public decimal CurrentBalance { get; set; }
    }
}
