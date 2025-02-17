using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class TransactionHead
    {
        public int HeadId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HeadName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Type { get; set; }

        public bool IsMandatory { get; set; }

        public string? Description { get; set; }

        [Required]
        public int ParishId { get; set; }

        public double? Aramanapct { get; set; }

        [MaxLength(10)]
        public string? Ordr { get; set; }

        [MaxLength(100)]
        public string? HeadNameMl { get; set; }

        [JsonIgnore]
        public Parish? Parish { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
