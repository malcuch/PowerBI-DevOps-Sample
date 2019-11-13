using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ReportDeployer.Console
{
    public class Deployer
    {
        private readonly PowerBIClientFactory powerBIClientFactory;
        private IOptions options;
        private Model.ReportCollection reportCollection;
        private PowerBIClient client;                       
        private Guid groupId;                          


        public Deployer(IOptions options)
        {
            powerBIClientFactory = new PowerBIClientFactory(options.PowerBiConnection);
            this.options = options;
            this.reportCollection = options.ReportFilesDiscovery.CreateReportCollection();             
        }

        public async Task Deploy()
        {
            client = await powerBIClientFactory.CreatePowerBIClient();
            IdentifyGroup();

            foreach (var report in reportCollection.Reports)
                InstallReport(report);

            if (!String.IsNullOrEmpty(options.OutSettingsFilePath))
                reportCollection.Save(options.OutSettingsFilePath, options.OutJsonObjectPath);
        }

        void IdentifyGroup()
        {
            Group group;
            var groups = client.Groups.GetGroups().Value;
            
            System.Console.WriteLine($"Retrieved {groups.Count} group(s)");
                      
            bool isValidGuid = Guid.TryParse(options.PowerBiConnection.GroupNameOrId, out groupId);

            if (isValidGuid)
            {
                System.Console.WriteLine($"Looking up group Id={groupId}");
                group = groups.Single(x => string.Equals(x.Id, groupId.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                System.Console.WriteLine($"Looking up group by name '{options.PowerBiConnection.GroupNameOrId}'");
                group = FindGroupByName(groups, options.PowerBiConnection.GroupNameOrId);
            }

            groupId = Guid.Parse(group.Id);

            System.Console.WriteLine($"Group ID: {groupId}");
        }

        Group FindGroupByName(IList<Group> groups, string groupName)
        {
            var matchedGroupsByName = groups.Where(x => x.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matchedGroupsByName.Count > 1)
                throw new InvalidOperationException($"Ambigous group name '{groupName}'. There are multiple ({matchedGroupsByName.Count}) groups with this name.");
            if (matchedGroupsByName.Count == 0)
                throw new InvalidOperationException($"No group found matching '{groupName}'");

            return matchedGroupsByName.Single();
        }



        private void InstallReport(Model.Report reportInfo)
        {
            System.Console.WriteLine($"Installing report {reportInfo}");

            Report report = FindExistingReport(reportInfo);

            if (report == null || options.OverwriteExistingReport)
                report = UploadReport(reportInfo);
            else
                System.Console.WriteLine($"Skip upload of the report. The report already exists in target group.");

            reportInfo.Id = report.Id;

            System.Console.WriteLine(
                $"   > Updating data source connection for dataset ID={report.DatasetId}; " +
                $"Server={options.DataSourceConnection.DatabaseServer}; Database={options.DataSourceConnection.DatabaseName}; Username={options.DataSourceConnection.DatabaseUser}");

            UpdateDataSourceConnection(report.DatasetId);

            System.Console.WriteLine($"   > Refreshing the dataset ID={report.DatasetId}");
            client.Datasets.RefreshDatasetInGroupAsync(groupId.ToString(), report.DatasetId);
        }
        
        private Report UploadReport(Model.Report report)
        {            
            var reportFilePath = reportCollection.GetReportFilePath(report);
            using (var pbixFile = File.OpenRead(reportFilePath))
            {

                var import = client.Imports.PostImportWithFileInGroup(
                    groupId.ToString(), 
                    pbixFile, 
                    report.GetPowerBIReportName(), 
                    "CreateOrOverwrite",
                    true);

                // wait for import
                import = WaitImportSucceeded(import);

                return client.Reports.GetReportInGroup(groupId.ToString(), import.Reports.Single().Id);
            }
        }

        private void UpdateDataSourceConnection(string datasetId)
        {            
            try
            {
                // update datasource
                client.Datasets.UpdateDatasourcesInGroup(groupId.ToString(), datasetId,
                    new UpdateDatasourcesRequest(
                        new UpdateDatasourceConnectionRequest(
                        new DatasourceConnectionDetails
                        {
                            Database = options.DataSourceConnection.DatabaseName,
                            Server = options.DataSourceConnection.DatabaseServer
                        }
                    )));

                // update dataset connection credentials (basic authorization)
                var dataSource = client.Datasets.GetGatewayDatasourcesInGroup(groupId.ToString(), datasetId).Value.Single();
                var basicCredentials = "{\"credentialData\":[{\"name\":\"username\",\"value\":\"" + options.DataSourceConnection.DatabaseUser + "\"},{\"name\":\"password\", \"value\":\"" + options.DataSourceConnection.DatabasePassword + "\"}]}";

                client.Gateways.UpdateDatasource(dataSource.GatewayId, dataSource.Id,
                    new UpdateDatasourceRequest(new CredentialDetails(basicCredentials, "Basic", "Encrypted", "None", "None")));
            }
            catch (HttpOperationException e)
            {
                System.Console.WriteLine($"ERROR: http response code {e.Response.StatusCode}\n{e.Response.Content}");
                throw e;
            }
        }

        private Report FindExistingReport(Model.Report reportInfo) => client.Reports.GetReportsInGroup(groupId.ToString()).Value.SingleOrDefault(r => r.Name == reportInfo.GetPowerBIReportName());

        
        private Import WaitImportSucceeded(Import import)
        {
            DateTime timeout = DateTime.UtcNow.AddSeconds(30);
            
            while (import.ImportState != "Succeeded")
            {
                import = client.Imports.GetImportByIdInGroup(groupId.ToString(), import.Id);

                if (DateTime.UtcNow > timeout)
                    throw new InvalidOperationException($"Timeout occured while waiting for report import. The import state is {import.ImportState}");
            }                      

            return import;
        }
    }
}
