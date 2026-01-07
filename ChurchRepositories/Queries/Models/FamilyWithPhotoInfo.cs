using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchRepositories.Queries.Models
{
    public class FamilyWithPhotoInfo
    {
        public Family Family { get; set; } = null!;
        public bool HasFamilyPhoto { get; set; }
        public Guid? FamilyPhotoFileId { get; set; }
    }

}
