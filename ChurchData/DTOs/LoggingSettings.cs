using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData.DTOs
{
    public class LoggingSettings
    {
        public bool EnableChangeLogging { get; set; }
        public Dictionary<string, bool> TableLogging { get; set; }
    }
}
