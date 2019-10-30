using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public interface INationalSocietyService
    {
        Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties();
        Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id);
        Task<Result<int>> CreateNationalSociety(CreateNationalSocietyRequestDto nationalSociety);
        Task<Result> EditNationalSociety(EditNationalSocietyRequestDto nationalSociety);
        Task<Result> RemoveNationalSociety(int id);
        Task<Result<List<GatewaySettingResponseDto>>> GetSmsGateways(int nationalSocietyId);
        Task<Result<int>> AddSmsGateway(int nationalSocietyId, GatewaySettingRequestDto gatewaySettingRequestDto);
        Task<Result> UpdateSmsGateway(int gatewaySettingId, GatewaySettingRequestDto gatewaySettingRequestDto);
        Task<Result> DeleteSmsGateway(int gatewaySettingId);
        Task UpdateAuthorizedApiKeys();
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IConfig _config;

        public NationalSocietyService(INyssContext context, ILoggerAdapter loggerAdapter, IConfig config)
        {
            _nyssContext = context;
            _loggerAdapter = loggerAdapter;
            _config = config;
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties()
        {
            try
            {
                var list = await _nyssContext.NationalSocieties
                    .Select(n => new NationalSocietyListResponseDto
                    {
                        Id = n.Id,
                        ContentLanguage = n.ContentLanguage.DisplayName,
                        Name = n.Name,
                        Country = n.Country.Name,
                        StartDate = n.StartDate
                    })
                    .OrderBy(n => n.Name)
                    .ToListAsync();

                return Success(list);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<List<NationalSocietyListResponseDto>>();
            }
        }

        public async Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties
                    .Select(n => new NationalSocietyResponseDto
                    {
                        Id = n.Id,
                        ContentLanguageId = n.ContentLanguage.Id,
                        ContentLanguageName = n.ContentLanguage.DisplayName,
                        Name = n.Name,
                        CountryId = n.Country.Id,
                        CountryName = n.Country.Name
                    })
                    .FirstOrDefaultAsync(n => n.Id == id);

                return Success(nationalSociety);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<NationalSocietyResponseDto>();
            }
        }

        public async Task<Result<int>> CreateNationalSociety(CreateNationalSocietyRequestDto dto)
        {
            try
            {
                if (_nyssContext.NationalSocieties.Any(ns => ns.Name.ToLower() == dto.Name.ToLower()))
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                }

                var nationalSociety = new Nyss.Data.Models.NationalSociety
                {
                    Name = dto.Name,
                    ContentLanguage = await GetLanguageById(dto.ContentLanguageId),
                    Country = await GetCountryById(dto.CountryId),
                    IsArchived = false,
                    StartDate = DateTime.UtcNow
                };

                if (nationalSociety.ContentLanguage == null)
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.LanguageNotFound);
                }

                if (nationalSociety.Country == null)
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.CountryNotFound);
                }

                var entity = await _nyssContext.AddAsync(nationalSociety);
                await _nyssContext.SaveChangesAsync();
                _loggerAdapter.Info($"A national society {nationalSociety} was created");
                return Success(entity.Entity.Id, ResultKey.NationalSociety.Creation.Success);
            }
            catch (Exception e)
            {
                _loggerAdapter.Debug(e);
                return HandleException(e).Cast<int>();
            }
        }

        public async Task<Result> EditNationalSociety(EditNationalSocietyRequestDto dto)
        {
            try
            {
                if (_nyssContext.NationalSocieties.Any(ns => ns.Id != dto.Id && ns.Name.ToLower() == dto.Name.ToLower()))
                {
                    return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
                }

                var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(dto.Id);

                nationalSociety.Name = dto.Name;
                nationalSociety.ContentLanguage = await GetLanguageById(dto.ContentLanguageId);
                nationalSociety.Country = await GetCountryById(dto.CountryId);

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.NationalSociety.Edit.Success);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        public async Task<Result> RemoveNationalSociety(int id)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(id);
                _nyssContext.NationalSocieties.Remove(nationalSociety);
                await _nyssContext.SaveChangesAsync();
                return SuccessMessage(ResultKey.NationalSociety.Remove.Success);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        public async Task<ContentLanguage> GetLanguageById(int id) =>
            await _nyssContext.ContentLanguages.FindAsync(id);

        public async Task<Country> GetCountryById(int id) => 
            await _nyssContext.Countries.FindAsync(id);

        public Result HandleException(Exception e)
        {
            if (e.InnerException is SqlException sqlException)
            {
                if (sqlException.Number == 2627 || sqlException.Number == 2601) // national society name already exists
                {
                    return Error(ResultKey.NationalSociety.Creation.NameAlreadyExists);                
                }
            }
            return Error(ResultKey.Shared.GeneralErrorMessage);
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

        public async Task<Result> UpdateSmsGateway(int gatewaySettingId, GatewaySettingRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var apiKeyExists = await _nyssContext.GatewaySettings.AnyAsync(gs => gs.ApiKey == gatewaySettingRequestDto.ApiKey && gs.Id != gatewaySettingId);

                if (apiKeyExists)
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.ApiKeyAlreadyExists);
                }

                var gatewaySettingToUpdate = await _nyssContext.GatewaySettings.FirstOrDefaultAsync(gs => gs.Id == gatewaySettingId);

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

        public async Task<Result> DeleteSmsGateway(int gatewaySettingId)
        {
            try
            {
                var gatewaySettingToDelete = await _nyssContext.GatewaySettings.FirstOrDefaultAsync(gs => gs.Id == gatewaySettingId);

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
