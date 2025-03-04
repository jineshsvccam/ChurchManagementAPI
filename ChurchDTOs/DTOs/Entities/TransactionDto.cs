using ChurchDTOs.DTOs.Utils;

public class TransactionDto : IParishEntity
{
    public string? Action { get; set; } // Action is included in DTO
    public int TransactionId { get; set; }
    public DateTime TrDate { get; set; }
    public string? VrNo { get; set; }
    public string? TransactionType { get; set; }
    public decimal IncomeAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public string? Description { get; set; }
    public int? HeadId { get; set; }
    public int? FamilyId { get; set; }
    public int? BankId { get; set; }
    public int ParishId { get; set; }
    public string? BillName { get; set; }



    // Optional: If you need these for client-side consumption, you can add them
    //public string? HeadName { get; set; }
    //public string? BankName { get; set; }
    //public string? FamilyName { get; set; }
}
