using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Features.SmsGateways
{
    public interface IIotHubService
    {
        Task<string> GetConnectionString(string deviceName);
        Task<Result> Ping(string gatewayDeviceIotHubDeviceName);
        Task<IEnumerable<string>> ListDevices();
    }

    public class IotHubService : IIotHubService
    {
        private readonly RegistryManager _registry;
        private readonly string _ioTHubHostName;
        private readonly ServiceClient _iotHubServiceClient;
        private readonly ILoggerAdapter _loggerAdapter;

        public IotHubService(INyssWebConfig config, ILoggerAdapter loggerAdapter)
        {
            _loggerAdapter = loggerAdapter;
            _registry = RegistryManager.CreateFromConnectionString(config.ConnectionStrings.IotHubManagement);
            _iotHubServiceClient = ServiceClient.CreateFromConnectionString(config.ConnectionStrings.IotHubService);

            _ioTHubHostName = IotHubConnectionStringBuilder.Create(config.ConnectionStrings.IotHubManagement).HostName;
        }

        public async Task<string> GetConnectionString(string deviceName)
        {
            var azureDevice = await _registry.GetDeviceAsync(deviceName);

            var key = azureDevice.Authentication.SymmetricKey.PrimaryKey;
            return $"HostName={_ioTHubHostName};DeviceId={deviceName};SharedAccessKey={key}";
        }

        public async Task<Result> Ping(string gatewayDeviceIotHubDeviceName)
        {
            var cloudToDeviceMethod = new CloudToDeviceMethod("ping_device", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            try
            {
                var response = await _iotHubServiceClient.InvokeDeviceMethodAsync(gatewayDeviceIotHubDeviceName, cloudToDeviceMethod);
                return response.Status == 200
                    ? Result.Success(response.GetPayloadAsJson())
                    : Result.Error(ResultKey.NationalSociety.SmsGateway.IoTHubPingFailed, response.GetPayloadAsJson());
            }
            // The `DeviceNotFoundException` is the one that is thrown for even the timeoutExceptions for some reasons..
            catch (DeviceNotFoundException ex)
            {
                _loggerAdapter.Error(ex);
                return Result.Error(ResultKey.NationalSociety.SmsGateway.IoTHubPingFailed);
            }
        }

        public async Task<IEnumerable<string>> ListDevices()
        {
            var query = _registry.CreateQuery("select * from devices", 1000);

            var allDevices = new List<string>();
            while (query.HasMoreResults)
            {
                var devices = await query.GetNextAsTwinAsync();
                allDevices.AddRange(devices.Select(x => x.DeviceId));
            }

            return allDevices.AsEnumerable();
        }
    }
}
