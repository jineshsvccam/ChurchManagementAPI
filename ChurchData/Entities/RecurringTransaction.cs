using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData
{
    public class RecurringTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RepeatedEntryId { get; set; }

        [Required]
        public int HeadId { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required]
        public int ParishId { get; set; }

        [Required]
        public string BillName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "numeric(15,2)")]
        public decimal IncomeAmount { get; set; }
    }
}
