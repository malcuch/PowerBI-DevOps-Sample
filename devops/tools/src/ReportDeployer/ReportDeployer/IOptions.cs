using ReportDeployer.Settings;

namespace ReportDeployer.Console
{
    public interface IOptions
    {
        PowerBiConnectionSettings PowerBiConnection { get; }

        SqlDbConnectionSettings DataSourceConnection { get; }
                       
        ReportFilesDiscoverySettings ReportFilesDiscovery { get; }


        string OutSettingsFilePath { get; }

        string OutJsonObjectPath { get; }

        bool OverwriteExistingReport { get; }
    }
}