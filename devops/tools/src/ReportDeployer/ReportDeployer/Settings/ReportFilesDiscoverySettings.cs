using ReportDeployer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportDeployer.Settings
{
    public class ReportFilesDiscoverySettings
    {
        public string Directory { get; set; }

        public string FileSearchPattern { get; set;  }

        public string InSettingsFilePath { get; set;  }

        internal ReportCollection CreateReportCollection()
        {
            if (!string.IsNullOrEmpty(InSettingsFilePath))
                return ReportCollection.LoadFromSettingsFile(InSettingsFilePath);
            else if (!string.IsNullOrEmpty(Directory))
                return ReportCollection.ScanDirectory(Directory, FileSearchPattern);
            else throw new ArgumentException("Neither reports settings file or directory were specified.");
        }
    }
}
