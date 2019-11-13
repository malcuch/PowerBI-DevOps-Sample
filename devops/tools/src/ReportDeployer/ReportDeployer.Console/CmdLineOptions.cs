using CommandLine;
using ReportDeployer.Settings;

namespace ReportDeployer.Console
{
    class CmdLineOptions : IOptions
    {
        [Option("tenantid", Required = true,
            HelpText = "The Azure AD tenant ID")]
        public string TenantId { get; set; }

        [Option("spid", Required = true,
            HelpText = "The Application ID of service principal that is used to authenticate to PowerBI API. This service principal must have permission to the target group")]
        public string ServicePrincipalAppId { get; set; }

        [Option("spsecret", Required = true,
            HelpText = "The secret for service principal")]
        public string ServicePrincipalSecret { get; set; }

        [Option('g', "pbigroup", Required = true,
            HelpText = "The PowerBI group (workspace) name or ID eg. 'Ecosystem Test Apleona' ")]
        public string GroupNameOrId { get; set; }

        [Option("overwrite", Required = false, Default = true,
            HelpText = "Indicates whether report with the same name will be overwritten on target PowerBI group.")]
        public bool OverwriteExistingReport { get; set; }

        [Option('d', "dir", Required = false,
            HelpText = "Required when insettings parameter is not provided. The path to a folder containing the report .pbix file(s)")]
        public string Directory { get; set; }

        [Option('f', "file", Required = false, Default = "*.pbix",
            HelpText = "Search pattern for the report .pbix files. Multiple patterns can be combined using pipe '|' character e.g. Report1.pbix|Report2.pbix")]
        public string FileSearchPattern { get; set; }

        [Option("insettings", Required = false, Default = null,
            HelpText = "Path to input JSON file containing the settings for reports to be uploaded")]
        public string InSettingsFilePath { get; set; }

        [Option("outsettings", Required = false, Default = "reportsettings.out.json",
             HelpText = "Path to output JSON file containing the uploaded report metadata and settings")]
        public string OutSettingsFilePath { get; set; }

        [Option("jsonpath", Required = false, Default = "PowerBI/Reports",
            HelpText = "The JSON object hierarchy path inside output settings file where reports data will be inserted")]
        public string OutJsonObjectPath { get; set; }

        [Option("ds.dbserver", Required = true,
            HelpText = "The report data source database server (SQL Server) name.")]
        public string DatabaseServer { get; set; }

        [Option("ds.dbname", Required = true,
            HelpText = "The report data source database name.")]
        public string DatabaseName { get; set; }

        [Option("ds.dbuser", Required = true,
            HelpText = "The report data source database user name.")]
        public string DatabaseUser { get; set; }

        [Option("ds.dbpassword", Required = true,
            HelpText = "The report data source database user password.")]
        public string DatabasePassword { get; set; }


        public PowerBiConnectionSettings PowerBiConnection => new PowerBiConnectionSettings
        {
            TenantId = TenantId,
            ServicePrincipalAppId = ServicePrincipalAppId,
            ServicePrincipalSecret = ServicePrincipalSecret,
            GroupNameOrId = GroupNameOrId
        };

        public SqlDbConnectionSettings DataSourceConnection => new SqlDbConnectionSettings
        {
            DatabaseServer = DatabaseServer,
            DatabaseName = DatabaseName,
            DatabaseUser = DatabaseUser,
            DatabasePassword = DatabasePassword
        };

        public ReportFilesDiscoverySettings ReportFilesDiscovery => new ReportFilesDiscoverySettings
        {
            Directory = Directory,
            FileSearchPattern = FileSearchPattern,
            InSettingsFilePath = InSettingsFilePath
        };
    }
}
