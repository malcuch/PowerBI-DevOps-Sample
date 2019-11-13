using System;
using System.Collections.Generic;
using System.Text;

namespace ReportDeployer.Settings
{
    public class SqlDbConnectionSettings
    {
        public string DatabaseServer { get; set; }

        public string DatabaseName { get; set; }

        public string DatabaseUser { get; set; }

        public string DatabasePassword { get; set; }
    }
}
