using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData
{
    public class Bank
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string Action { get; set; } // INSERT or UPDATE
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public int ParishId { get; set; }

        public Parish? Parish { get; set; }
        public ICollection<Transaction> Transactions { get; set; }=new List<Transaction>();
    }
}
