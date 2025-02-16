using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class Transaction
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string? Action { get; set; } // INSERT or UPDATE
        public int TransactionId { get; set; }
        public DateTime TrDate { get; set; }
        public string? VrNo { get; set; }
        public string? TransactionType { get; set; }
        public int? HeadId { get; set; }
        public int? FamilyId { get; set; }
        public int? BankId { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string? Description { get; set; }
        public int ParishId { get; set; }

        [JsonIgnore]
        public TransactionHead? TransactionHead { get; set; }
        [JsonIgnore]
        public Family? Family { get; set; }
        [JsonIgnore]
        public Bank? Bank { get; set; }
        [JsonIgnore]
        public Parish? Parish { get; set; }
    }
}
