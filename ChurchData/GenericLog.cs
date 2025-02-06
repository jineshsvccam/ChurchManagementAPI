using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class GenericLog
    {
        public int LogId { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string ChangeType { get; set; }
        public int? ChangedBy { get; set; }
        public DateTime ChangeTimestamp { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}


