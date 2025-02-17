using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class Family
    {
        public int FamilyId { get; set; }

        public int UnitId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FamilyName { get; set; } = string.Empty;

        public string? Address { get; set; }

        [MaxLength(100)]
        public string? ContactInfo { get; set; }

        [MaxLength(10)]
        public string? Category { get; set; }

        public int FamilyNumber { get; set; }

        [MaxLength(10)]
        public string? Status { get; set; }

        [Required]
        [MaxLength(50)]
        public string HeadName { get; set; } = string.Empty;

        public int ParishId { get; set; }

        public DateTime? JoinDate { get; set; }

        [JsonIgnore]
        public Unit? Unit { get; set; }

        [JsonIgnore]
        public Parish? Parish { get; set; }

        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
