using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.SmsGateway.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.SmsGateway
{
    public interface ISmsGatewayService
    {
        Task<Result<GatewaySettingResponseDto>> GetSmsGateway(int smsGatewayId);
        Task<Result<List<GatewaySettingResponseDto>>> GetSmsGateways(int nationalSocietyId);
        Task<Result<int>> AddSmsGateway(int nationalSocietyId, GatewaySettingRequestDto gatewaySettingRequestDto);
        Task<Result> UpdateSmsGateway(int smsGatewayId, GatewaySettingRequestDto gatewaySettingRequestDto);
        Task<Result> DeleteSmsGateway(int smsGatewayId);
        Task UpdateAuthorizedApiKeys();
    }

    public class SmsGatewayService : ISmsGatewayService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IConfig _config;
        private readonly ISmsGatewayBlobProvider _smsGatewayBlobProvider;

        public SmsGatewayService(INyssContext nyssContext, ILoggerAdapter loggerAdapter, IConfig config, ISmsGatewayBlobProvider smsGatewayBlobProvider)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _smsGatewayBlobProvider = smsGatewayBlobProvider;
        }

        public async Task<Result<GatewaySettingResponseDto>> GetSmsGateway(int smsGatewayId)
        {
            var gatewaySetting = await _nyssContext.GatewaySettings
                .Select(gs => new GatewaySettingResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    ApiKey = gs.ApiKey,
                    GatewayType = gs.GatewayType,
                    EmailAddress = gs.EmailAddress
                })
                .FirstOrDefaultAsync(gs => gs.Id == smsGatewayId);

            if (gatewaySetting == null)
            {
                return Error<GatewaySettingResponseDto>(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
            }

            var result = Success(gatewaySetting);

            return result;
        }

        public async Task<Result<List<GatewaySettingResponseDto>>> GetSmsGateways(int nationalSocietyId)
        {
            var gatewaySettings = await _nyssContext.GatewaySettings
                .Where(gs => gs.NationalSocietyId == nationalSocietyId)
                .OrderBy(gs => gs.Id)
                .Select(gs => new GatewaySettingResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    ApiKey = gs.ApiKey,
                    GatewayType = gs.GatewayType
                })
                .ToListAsync();

            var result = Success(gatewaySettings);

            return result;
        }

        public async Task<Result<int>> AddSmsGateway(int nationalSocietyId, GatewaySettingRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var nationalSocietyExists = await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId);

                if (!nationalSocietyExists)
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
                }

                var apiKeyExists = await _nyssContext.GatewaySettings.AnyAsync(gs => gs.ApiKey == gatewaySettingRequestDto.ApiKey);

                if (apiKeyExists)
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
                }

                var gatewaySettingToAdd = new GatewaySetting
                {
                    Name = gatewaySettingRequestDto.Name,
                    ApiKey = gatewaySettingRequestDto.ApiKey,
                    GatewayType = gatewaySettingRequestDto.GatewayType,
                    EmailAddress = gatewaySettingRequestDto.EmailAddress,
                    NationalSocietyId = nationalSocietyId
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

        public async Task<Result> UpdateSmsGateway(int smsGatewayId, GatewaySettingRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var gatewaySettingToUpdate = await _nyssContext.GatewaySettings.FindAsync(smsGatewayId);

                if (gatewaySettingToUpdate == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                var apiKeyExists = await _nyssContext.GatewaySettings.AnyAsync(gs => gs.ApiKey == gatewaySettingRequestDto.ApiKey && gs.Id != smsGatewayId);

                if (apiKeyExists)
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
                }

                gatewaySettingToUpdate.Name = gatewaySettingRequestDto.Name;
                gatewaySettingToUpdate.ApiKey = gatewaySettingRequestDto.ApiKey;
                gatewaySettingToUpdate.GatewayType = gatewaySettingRequestDto.GatewayType;
                gatewaySettingToUpdate.EmailAddress = gatewaySettingRequestDto.EmailAddress;

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

        public async Task<Result> DeleteSmsGateway(int smsGatewayId)
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
    }
}
