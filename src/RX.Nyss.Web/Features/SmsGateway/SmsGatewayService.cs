using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.SmsGateway.Dto;
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

        public SmsGatewayService(INyssContext context, ILoggerAdapter loggerAdapter, IConfig config)
        {
            _nyssContext = context;
            _loggerAdapter = loggerAdapter;
            _config = config;
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
                    NationalSocietyId = gs.NationalSociety.Id
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
                var nationalSocietyExists = await _nyssContext.NationalSocieties.AnyAsync(n => n.Id == nationalSocietyId);

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
                var apiKeyExists = await _nyssContext.GatewaySettings.AnyAsync(gs => gs.ApiKey == gatewaySettingRequestDto.ApiKey && gs.Id != smsGatewayId);

                if (apiKeyExists)
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
                }

                var gatewaySettingToUpdate = await _nyssContext.GatewaySettings.FindAsync(smsGatewayId);

                if (gatewaySettingToUpdate == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                gatewaySettingToUpdate.Name = gatewaySettingRequestDto.Name;
                gatewaySettingToUpdate.ApiKey = gatewaySettingRequestDto.ApiKey;
                gatewaySettingToUpdate.GatewayType = gatewaySettingRequestDto.GatewayType;

                await _nyssContext.SaveChangesAsync();

                await UpdateAuthorizedApiKeys();

                return Success(ResultKey.NationalSociety.SmsGateway.SuccessfullyUpdated);
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
                var gatewaySettingToDelete = await _nyssContext.GatewaySettings.FirstOrDefaultAsync(gs => gs.Id == smsGatewayId);

                if (gatewaySettingToDelete == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                _nyssContext.GatewaySettings.Remove(gatewaySettingToDelete);
                await _nyssContext.SaveChangesAsync();

                await UpdateAuthorizedApiKeys();

                return Success(ResultKey.NationalSociety.SmsGateway.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task UpdateAuthorizedApiKeys()
        {
            var storageAccountConnectionString = _config.ConnectionStrings.SmsGatewayBlobContainer;
            var smsGatewayBlobContainerName = _config.SmsGatewayBlobContainerName;
            var authorizedApiKeysBlobObjectName = _config.AuthorizedApiKeysBlobObjectName;

            if (string.IsNullOrWhiteSpace(smsGatewayBlobContainerName) ||
                string.IsNullOrWhiteSpace(authorizedApiKeysBlobObjectName) ||
                !CloudStorageAccount.TryParse(storageAccountConnectionString, out var storageAccount))
            {
                _loggerAdapter.Error("Unable to update authorized API keys. A configuration of a blob storage is not valid.");
                throw new ResultException(ResultKey.UnexpectedError);
            }

            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var smsGatewayContainer = cloudBlobClient.GetContainerReference(smsGatewayBlobContainerName);
            var authorizedApiKeysBlob = smsGatewayContainer.GetBlockBlobReference(authorizedApiKeysBlobObjectName);

            var authorizedApiKeys = await _nyssContext.GatewaySettings
                .OrderBy(gs => gs.NationalSocietyId)
                .ThenBy(gs => gs.Id)
                .Select(gs => gs.ApiKey)
                .ToListAsync();

            var blobContentToUpload = string.Join(Environment.NewLine, authorizedApiKeys);

            await authorizedApiKeysBlob.UploadTextAsync(blobContentToUpload);
        }
    }
}
