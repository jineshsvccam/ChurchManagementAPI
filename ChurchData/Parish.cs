using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class Parish
    {
        public int ParishId { get; set; }
        public string ParishName { get; set; }
        public string ParishLocation { get; set; }
        public string Photo { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Place { get; set; }
        public string Pincode { get; set; }
        public string VicarName { get; set; }
        public int DistrictId { get; set; }
        public District? District { get; set; } // Optional property
        public ICollection<Unit> Units { get; set; } = new List<Unit>();

    }

}
