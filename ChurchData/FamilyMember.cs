using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class FamilyMember
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string Action { get; set; } // INSERT or UPDATE
        public int MemberId { get; set; }
        public int FamilyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactInfo { get; set; }
        public string Role { get; set; }

        [JsonIgnore]
        public Family? Family { get; set; }
    }
}
