using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchCommon.Utils
{
    public class LoggingSettings
    {
        public bool EnableChangeLogging { get; set; }
        public Dictionary<string, bool> TableLogging { get; set; }
    }
}
