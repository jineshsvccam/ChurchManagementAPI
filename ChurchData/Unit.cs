using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class Unit
    {
        public int UnitId { get; set; }
        public int ParishId { get; set; }
        public string UnitName { get; set; }
        public string? Description { get; set; }
        public string? UnitPresident { get; set; }
        public string? UnitSecretary { get; set; }
        public Parish? Parish { get; set; }
        
        public ICollection<Family> Families { get; set; } = new List<Family>();
    }

}
