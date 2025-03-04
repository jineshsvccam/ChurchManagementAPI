using ChurchData;
using System.Text.Json.Serialization;

using System;
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
    public decimal IncomeAmount { get; set; } // Defaults to 0
    public decimal ExpenseAmount { get; set; } // Defaults to 0
    public string? Description { get; set; }
    public int ParishId { get; set; }

    // Newly added columns
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ensures UTC time for consistency
    public Guid CreatedBy { get; set; } // UUID (non-nullable)
    public DateTime? UpdatedAt { get; set; } // Nullable, updated only on changes
    public Guid? UpdatedBy { get; set; } // Nullable, updated only on changes
    public string? BillName { get; set; }

    // Navigation properties (ignored in JSON to avoid circular references)
    [JsonIgnore]
    public TransactionHead? TransactionHead { get; set; }

    [JsonIgnore]
    public Family? Family { get; set; }

    [JsonIgnore]
    public Bank? Bank { get; set; }

    [JsonIgnore]
    public Parish? Parish { get; set; }
}
