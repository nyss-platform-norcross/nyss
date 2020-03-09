using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Features.SmsGateways
{
    public interface IIotHubService
    {
        Task CreateDevice(string deviceName);
        Task<string> GetConnectionString(string deviceName);
        Task RemoveDevice(string deviceName);
        Task<Result> Ping(string gatewayDeviceIotHubDeviceName);
    }

    public class IotHubService : IIotHubService
    {
        private readonly RegistryManager _registry;
        private readonly string _ioTHubHostName;
        private readonly ServiceClient _iotHubServiceClient;

        public IotHubService(INyssWebConfig config)
        {
            _registry = RegistryManager.CreateFromConnectionString(config.ConnectionStrings.IotHubManagement);
            _iotHubServiceClient = ServiceClient.CreateFromConnectionString(config.ConnectionStrings.IotHubService);

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

        public async Task<Result> Ping(string gatewayDeviceIotHubDeviceName)
        {
            var cloudToDeviceMethod = new CloudToDeviceMethod("ping_device", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            var response = await _iotHubServiceClient.InvokeDeviceMethodAsync(gatewayDeviceIotHubDeviceName, cloudToDeviceMethod);

            if (response.Status == 200)
            {
                return Result.Success(response.GetPayloadAsJson());
            }

            return Result.Error(response.GetPayloadAsJson());
        }
    }
}
