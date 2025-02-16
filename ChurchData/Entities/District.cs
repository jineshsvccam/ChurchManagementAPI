using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class District
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int DioceseId { get; set; }
        public string Description { get; set; }
        public Diocese? Diocese { get; set; }
        public ICollection<Parish> Parishes { get; set; } = new List<Parish>();
    }

}
