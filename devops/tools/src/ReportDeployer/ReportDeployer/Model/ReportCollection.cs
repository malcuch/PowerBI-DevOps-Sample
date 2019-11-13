using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportDeployer.Model
{
    internal class ReportCollection
    {
        private string baseDirectory;
        private List<Report> reports = new List<Report>();

        public IEnumerable<Report> Reports => reports.Cast<Report>();

        public static ReportCollection LoadFromSettingsFile(string filePath)
        {
            System.Console.WriteLine($"Loading report from settings file at {filePath}");

            return new ReportCollection
            {
                baseDirectory = Path.GetDirectoryName(filePath),
                reports = JsonConvert.DeserializeObject<List<Report>>(File.ReadAllText(filePath))
            };
        }

        public static ReportCollection ScanDirectory(string directoryPath, string fileSearchPattern)
        {
            System.Console.WriteLine($"Scaning directory {directoryPath} for matching report files by pattern {fileSearchPattern}");

            return new ReportCollection
            {
                baseDirectory = directoryPath,
                reports = fileSearchPattern.Split('|')
                    .SelectMany(
                        searchPattern => Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly)
                    )
                    .Select(
                        filePath => new Report
                        {
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            FileName = Path.GetRelativePath(directoryPath, filePath),
                        }
                    )
                    .ToList()
            };
        }

        public void Save(string filePath, string jsonObjectPath)
        {
            System.Console.WriteLine($"Saving report collection at {filePath}. JSON path {jsonObjectPath}");

            var data = jsonObjectPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Reverse()
                .Aggregate<string, object>(JToken.FromObject(reports), (result, name) => new JObject(new JProperty(name, JToken.FromObject(result))));
           
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(filePath, json);
        }

        internal string GetReportFilePath(Report report) => Path.GetFullPath(report.FileName, baseDirectory);
    }
}
