using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChurchData
{
    public class Diocese
    {
        public int DioceseId { get; set; }
        public string DioceseName { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
        public string Territory { get; set; }

        [JsonIgnore] // Exclude from serialization
        public ICollection<District> Districts { get; set; } = new List<District>();
    }


}
