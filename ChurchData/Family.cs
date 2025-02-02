using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChurchData
{
    public class Family
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string? Action { get; set; } // INSERT or UPDATE
        public int FamilyId { get; set; }
        public int UnitId { get; set; }
        public string? FamilyName { get; set; }
        public string? Address { get; set; }
        public string? ContactInfo { get; set; }
        public string? Category { get; set; }
        public int FamilyNumber { get; set; }
        public string? Status { get; set; }
        public string? HeadName { get; set; }
        public int ParishId { get; set; }
        public DateTime? JoinDate { get; set; }

        [JsonIgnore]
        public Unit? Unit { get; set; }
        [JsonIgnore]
        public Parish? Parish { get; set; }
        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public ICollection<Transaction> Transactions { get; set; }=new List<Transaction>();
    }   

}
