using System;
using System.Collections.Generic;
using System.Text;

namespace ReportDeployer.Settings
{
    public class PowerBiConnectionSettings
    {
        public string ServicePrincipalAppId { get; set; }

        public string ServicePrincipalSecret { get; set; }

        public string TenantId { get; set; }

        public string GroupNameOrId { get; set; }
    }
}
