using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Features.SmsGateways
{
    public interface IIotHubService
    {
        Task CreateDevice(string deviceName);
        Task<string> GetConnectionString(string deviceName);
        Task RemoveDevice(string deviceName);
    }

    public class IotHubService : IIotHubService
    {
        private readonly RegistryManager _registry;
        private readonly string _ioTHubHostName;

        public IotHubService(INyssWebConfig config)
        {
            _registry = RegistryManager.CreateFromConnectionString(config.ConnectionStrings.IotHubManagement);
            _ioTHubHostName = IotHubConnectionStringBuilder.Create(config.ConnectionStrings.IotHubManagement).HostName;
        }

        public async Task CreateDevice(string deviceName)
        {
            var device = new Device(deviceName) { Authentication = new AuthenticationMechanism { Type = AuthenticationType.Sas } };

            await _registry.AddDeviceAsync(device);
        }

        public async Task<string> GetConnectionString(string deviceName)
        {
            var azureDevice = await _registry.GetDeviceAsync(deviceName);

            var key = azureDevice.Authentication.SymmetricKey.PrimaryKey;
            return $"HostName={_ioTHubHostName};DeviceId={deviceName};SharedAccessKey={key}";
        }

        public async Task RemoveDevice(string deviceName) => await _registry.RemoveDeviceAsync(deviceName);
    }
}
