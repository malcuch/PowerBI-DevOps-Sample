using CommandLine;

namespace ReportDeployer.Console
{
    class Program
    {
        public static int Main(string[] args)
        {
            int exitCode = -1;

            Parser.Default.ParseArguments<CmdLineOptions>(args)
                .WithParsed(options =>
                {
                    var deployer = new Deployer(options);
                    deployer.Deploy().Wait();
                    exitCode = 0;
                });              
                        
            return exitCode;
        }
    }
}
