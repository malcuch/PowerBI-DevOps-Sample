
using System;
using System.IO;

namespace ReportDeployer.Model
{
    internal class Report
    {
        public string Id { get; set; }

        public string FileName { get; set; }

        public string Name { get; set; }

        public dynamic CustomProperties { get; set; }

        public override string ToString()
        {
            return $"{FileName}";
        }

        internal string GetPowerBIReportName() => Path.GetFileNameWithoutExtension(FileName);        
    }
}
