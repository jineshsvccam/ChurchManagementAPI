using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class DioceseDto
    {
        public int DioceseId { get; set; }
        public string DioceseName { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
        public string Territory { get; set; }
    }
}
