using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class UnitDto
    {
        public string? Action { get; set; } // INSERT or UPDATE
        public int UnitId { get; set; }
        public int ParishId { get; set; }
        public string? UnitName { get; set; }
        public string? Description { get; set; }
        public string? UnitPresident { get; set; }
        public string? UnitSecretary { get; set; }
    }


}
