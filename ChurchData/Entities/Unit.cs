using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChurchData
{
    public class Unit
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string Action { get; set; } // INSERT or UPDATE
        public int UnitId { get; set; }
        public int ParishId { get; set; }
        public string? UnitName { get; set; }
        public string? Description { get; set; }
        public string? UnitPresident { get; set; }
        public string? UnitSecretary { get; set; }
        [JsonIgnore]
        public Parish? Parish { get; set; }
        
        public ICollection<Family> Families { get; set; } = new List<Family>();
    }

}
