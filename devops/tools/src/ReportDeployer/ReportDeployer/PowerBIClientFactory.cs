using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.Rest;
using ReportDeployer.Settings;
using System.Threading.Tasks;

namespace ReportDeployer
{
    internal class PowerBIClientFactory
    {
        private readonly PowerBiConnectionSettings connectionSettings;

        public PowerBIClientFactory(PowerBiConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        internal async Task<PowerBIClient> CreatePowerBIClient()
        {
            var authResult = await Authenticate();
            var token = new TokenCredentials(authResult.AccessToken, "Bearer");
            return new PowerBIClient(token);
        }

        private async Task<AuthenticationResult> Authenticate()
        {
            var tenantSpecificUrl = $"https://login.microsoftonline.com/{connectionSettings.TenantId}";
            var authenticationContext = new AuthenticationContext(tenantSpecificUrl);

            var credential = new ClientCredential(connectionSettings.ServicePrincipalAppId, connectionSettings.ServicePrincipalSecret);
            var authenticationResult = await authenticationContext.AcquireTokenAsync("https://analysis.windows.net/powerbi/api", credential);
            return authenticationResult;
        }
    }
}
