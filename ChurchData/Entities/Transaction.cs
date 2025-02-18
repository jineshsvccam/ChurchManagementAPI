using ChurchData;
using System.Text.Json.Serialization;

public class Transaction
{
    public int TransactionId { get; set; }
    public DateTime TrDate { get; set; }
    public string? VrNo { get; set; }
    public string? TransactionType { get; set; }
    public int? HeadId { get; set; }
    public int? FamilyId { get; set; }
    public int? BankId { get; set; }
    public decimal IncomeAmount { get; set; } // Allow nullable if you want to store null
    public decimal ExpenseAmount { get; set; } // Allow nullable if you want to store null
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
