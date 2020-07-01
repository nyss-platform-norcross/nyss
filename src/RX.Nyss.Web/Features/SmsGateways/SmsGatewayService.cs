using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.SmsGateways
{
    public interface ISmsGatewayService
    {
        Task<Result<GatewaySettingResponseDto>> Get(int smsGatewayId);
        Task<Result<List<GatewaySettingResponseDto>>> List(int nationalSocietyId);
        Task<Result<int>> Create(int nationalSocietyId, GatewaySettingRequestDto gatewaySettingRequestDto);
        Task<Result> Edit(int smsGatewayId, GatewaySettingRequestDto gatewaySettingRequestDto);
        Task<Result> Delete(int smsGatewayId);
        Task UpdateAuthorizedApiKeys();
        Task<Result> GetIotHubConnectionString(int smsGatewayId);
        Task<Result<IEnumerable<string>>> ListIotHubDevices();
    }

    public class SmsGatewayService : ISmsGatewayService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly ISmsGatewayBlobProvider _smsGatewayBlobProvider;
        private readonly IIotHubService _iotHubService;

        public SmsGatewayService(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            ISmsGatewayBlobProvider smsGatewayBlobProvider, IIotHubService iotHubService)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _smsGatewayBlobProvider = smsGatewayBlobProvider;
            _iotHubService = iotHubService;
        }

        public async Task<Result<GatewaySettingResponseDto>> Get(int smsGatewayId)
        {
            var gatewaySetting = await _nyssContext.GatewaySettings
                .Select(gs => new GatewaySettingResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    ApiKey = gs.ApiKey,
                    GatewayType = gs.GatewayType,
                    EmailAddress = gs.EmailAddress,
                    IotHubDeviceName = gs.IotHubDeviceName
                })
                .FirstOrDefaultAsync(gs => gs.Id == smsGatewayId);

            if (gatewaySetting == null)
            {
                return Error<GatewaySettingResponseDto>(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
            }

            var result = Success(gatewaySetting);

            return result;
        }

        public async Task<Result<List<GatewaySettingResponseDto>>> List(int nationalSocietyId)
        {
            var gatewaySettings = await _nyssContext.GatewaySettings
                .Where(gs => gs.NationalSocietyId == nationalSocietyId)
                .OrderBy(gs => gs.Id)
                .Select(gs => new GatewaySettingResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    ApiKey = gs.ApiKey,
                    GatewayType = gs.GatewayType,
                    IotHubDeviceName = gs.IotHubDeviceName
                })
                .ToListAsync();

            var result = Success(gatewaySettings);

            return result;
        }

        public async Task<Result<int>> Create(int nationalSocietyId, GatewaySettingRequestDto gatewaySettingRequestDto)
        {
            try
            {
                if (!await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId))
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
                }

                var gatewaySettingToAdd = new GatewaySetting
                {
                    Name = gatewaySettingRequestDto.Name,
                    ApiKey = gatewaySettingRequestDto.ApiKey,
                    GatewayType = gatewaySettingRequestDto.GatewayType,
                    EmailAddress = gatewaySettingRequestDto.EmailAddress,
                    NationalSocietyId = nationalSocietyId,
                    IotHubDeviceName = gatewaySettingRequestDto.IotHubDeviceName
                };

                await _nyssContext.GatewaySettings.AddAsync(gatewaySettingToAdd);
                await _nyssContext.SaveChangesAsync();
                
                await UpdateAuthorizedApiKeys();

                return Success(gatewaySettingToAdd.Id, ResultKey.NationalSociety.SmsGateway.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> Edit(int smsGatewayId, GatewaySettingRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var gatewaySettingToUpdate = await _nyssContext.GatewaySettings
                    .Include(x => x.NationalSociety.Country)
                    .SingleOrDefaultAsync(x => x.Id == smsGatewayId);
                
                if (gatewaySettingToUpdate == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                gatewaySettingToUpdate.Name = gatewaySettingRequestDto.Name;
                gatewaySettingToUpdate.ApiKey = gatewaySettingRequestDto.ApiKey;
                gatewaySettingToUpdate.GatewayType = gatewaySettingRequestDto.GatewayType;
                gatewaySettingToUpdate.EmailAddress = gatewaySettingRequestDto.EmailAddress;
                gatewaySettingToUpdate.IotHubDeviceName = gatewaySettingRequestDto.IotHubDeviceName;

                await _nyssContext.SaveChangesAsync();
                await UpdateAuthorizedApiKeys();

                return SuccessMessage(ResultKey.NationalSociety.SmsGateway.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result> Delete(int smsGatewayId)
        {
            try
            {
                var gatewaySettingToDelete = await _nyssContext.GatewaySettings.FindAsync(smsGatewayId);

                if (gatewaySettingToDelete == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                _nyssContext.GatewaySettings.Remove(gatewaySettingToDelete);

                await _nyssContext.SaveChangesAsync();

                await UpdateAuthorizedApiKeys();
                
                return SuccessMessage(ResultKey.NationalSociety.SmsGateway.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task UpdateAuthorizedApiKeys()
        {
            var authorizedApiKeys = await _nyssContext.GatewaySettings
                .OrderBy(gs => gs.NationalSocietyId)
                .ThenBy(gs => gs.Id)
                .Select(gs => gs.ApiKey)
                .ToListAsync();

            var blobContentToUpload = string.Join(Environment.NewLine, authorizedApiKeys);
            await _smsGatewayBlobProvider.UpdateApiKeys(blobContentToUpload);
        }

        public async Task<Result> GetIotHubConnectionString(int smsGatewayId)
        {
            var gatewayDevice = await _nyssContext.GatewaySettings.FindAsync(smsGatewayId);

            if (string.IsNullOrEmpty(gatewayDevice?.IotHubDeviceName))
            {
                return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
            }

            var connectionString = await _iotHubService.GetConnectionString(gatewayDevice.IotHubDeviceName);

            return Success(connectionString);
        }
        
        public async Task<Result<IEnumerable<string>>> ListIotHubDevices()
        {
            var allDevices = await _iotHubService.ListDevices();

            var takenDevices = await _nyssContext.GatewaySettings
                .Where(sg => !string.IsNullOrEmpty(sg.IotHubDeviceName))
                .Select(sg => sg.IotHubDeviceName)
                .ToListAsync();

            var availableDevices = allDevices.Except(takenDevices);

            return Success(availableDevices);
        }
    }
}
