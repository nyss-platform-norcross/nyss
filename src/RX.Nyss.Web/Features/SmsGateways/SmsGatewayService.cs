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
using RX.Nyss.Data.Concepts;
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
        Task<Result<int>> Create(int nationalSocietyId, EditGatewaySettingRequestDto editGatewaySettingRequestDto);
        Task<Result> Edit(int smsGatewayId, EditGatewaySettingRequestDto editGatewaySettingRequestDto);
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
                    IotHubDeviceName = gs.IotHubDeviceName,
                    ModemOneName = gs.Modems != null && gs.Modems.Any(gm => gm.ModemId == 1)
                        ? gs.Modems.First(gm => gm.ModemId == 1).Name
                        : null,
                    ModemTwoName = gs.Modems != null && gs.Modems.Any(gm => gm.ModemId == 2)
                        ? gs.Modems.First(gm => gm.ModemId == 2).Name
                        : null
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

        public async Task<Result<int>> Create(int nationalSocietyId, EditGatewaySettingRequestDto editGatewaySettingRequestDto)
        {
            try
            {
                if (!await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId))
                {
                    return Error<int>(ResultKey.NationalSociety.SmsGateway.NationalSocietyDoesNotExist);
                }

                var gatewaySettingToAdd = new GatewaySetting
                {
                    Name = editGatewaySettingRequestDto.Name,
                    ApiKey = editGatewaySettingRequestDto.ApiKey,
                    GatewayType = editGatewaySettingRequestDto.GatewayType,
                    EmailAddress = editGatewaySettingRequestDto.EmailAddress,
                    NationalSocietyId = nationalSocietyId,
                    IotHubDeviceName = editGatewaySettingRequestDto.IotHubDeviceName
                };

                AttachGatewayModems(gatewaySettingToAdd, editGatewaySettingRequestDto);

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

        public async Task<Result> Edit(int smsGatewayId, EditGatewaySettingRequestDto editGatewaySettingRequestDto)
        {
            try
            {
                var gatewaySettingToUpdate = await _nyssContext.GatewaySettings
                    .Include(x => x.Modems)
                    .SingleOrDefaultAsync(x => x.Id == smsGatewayId);

                if (gatewaySettingToUpdate == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                gatewaySettingToUpdate.Name = editGatewaySettingRequestDto.Name;
                gatewaySettingToUpdate.ApiKey = editGatewaySettingRequestDto.ApiKey;
                gatewaySettingToUpdate.GatewayType = editGatewaySettingRequestDto.GatewayType;
                gatewaySettingToUpdate.EmailAddress = editGatewaySettingRequestDto.EmailAddress;
                gatewaySettingToUpdate.IotHubDeviceName = editGatewaySettingRequestDto.IotHubDeviceName;

                EditGatewayModems(gatewaySettingToUpdate, editGatewaySettingRequestDto);

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
                var gatewaySettingToDelete = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .SingleOrDefaultAsync(gs => gs.Id == smsGatewayId);

                if (gatewaySettingToDelete == null)
                {
                    return Error(ResultKey.NationalSociety.SmsGateway.SettingDoesNotExist);
                }

                var modems = gatewaySettingToDelete.Modems.ToList();

                if (modems.Any())
                {
                    RemoveManagerModemReferences(modems);
                    RemoveSupervisorModemReferences(modems);
                    RemoveHeadSupervisorModemReferences(modems);
                    RemoveTechnicalAdvisorModemReferences(modems);
                    RemoveAlertRecipientModemsReferences(modems);
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

        private void AttachGatewayModems(GatewaySetting gatewaySetting, EditGatewaySettingRequestDto dto)
        {
            if (!string.IsNullOrEmpty(dto.ModemOneName) && !string.IsNullOrEmpty(dto.ModemTwoName))
            {
                gatewaySetting.Modems = new List<GatewayModem>
                {
                    new GatewayModem
                    {
                        ModemId = 1,
                        Name = dto.ModemOneName
                    },
                    new GatewayModem
                    {
                        ModemId = 2,
                        Name = dto.ModemTwoName
                    }
                };
            }
        }

        private void EditGatewayModems(GatewaySetting gatewaySetting, EditGatewaySettingRequestDto dto)
        {
            if (!string.IsNullOrEmpty(dto.ModemOneName) && !string.IsNullOrEmpty(dto.ModemTwoName))
            {
                if (!gatewaySetting.Modems.Any())
                {
                    AttachGatewayModems(gatewaySetting, dto);
                }
                else
                {
                    gatewaySetting.Modems.First(gm => gm.ModemId == 1).Name = dto.ModemOneName;
                    gatewaySetting.Modems.First(gm => gm.ModemId == 2).Name = dto.ModemTwoName;
                }
            }
            else
            {
                var modemsToRemove = gatewaySetting.Modems.ToList();

                if (modemsToRemove.Any())
                {
                    RemoveAlertRecipientModemsReferences(modemsToRemove);
                    RemoveManagerModemReferences(modemsToRemove);
                    RemoveSupervisorModemReferences(modemsToRemove);
                    RemoveHeadSupervisorModemReferences(modemsToRemove);
                    RemoveTechnicalAdvisorModemReferences(modemsToRemove);

                    _nyssContext.GatewayModems.RemoveRange(modemsToRemove);
                }
            }
        }

        private void RemoveManagerModemReferences(List<GatewayModem> gatewayModems)
        {
            var managersConnectedToModems = _nyssContext.Users
                .Where(u => u.Role == Role.Manager)
                .Select(u => (ManagerUser)u)
                .Where(mu => gatewayModems.Contains(mu.Modem))
                .ToList();

            foreach (var manager in managersConnectedToModems)
            {
                manager.Modem = null;
            }
        }

        private void RemoveSupervisorModemReferences(List<GatewayModem> gatewayModems)
        {
            var supervisorsConnectedToModems = _nyssContext.Users
                .Where(u => u.Role == Role.Supervisor)
                .Select(u => (SupervisorUser)u)
                .Where(su => gatewayModems.Contains(su.Modem))
                .ToList();

            foreach (var supervisor in supervisorsConnectedToModems)
            {
                supervisor.Modem = null;
            }
        }

        private void RemoveHeadSupervisorModemReferences(List<GatewayModem> gatewayModems)
        {
            var headSupervisorsConnectedToModems = _nyssContext.Users
                .Where(u => u.Role == Role.HeadSupervisor)
                .Select(u => (HeadSupervisorUser)u)
                .Where(hsu => gatewayModems.Contains(hsu.Modem))
                .ToList();

            foreach (var headSupervisor in headSupervisorsConnectedToModems)
            {
                headSupervisor.Modem = null;
            }
        }

        private void RemoveTechnicalAdvisorModemReferences(List<GatewayModem> gatewayModems)
        {
            var technicalAdvisorModemsToRemove = _nyssContext.TechnicalAdvisorUserGatewayModems
                .Where(tam => gatewayModems.Contains(tam.GatewayModem))
                .ToList();

            _nyssContext.TechnicalAdvisorUserGatewayModems.RemoveRange(technicalAdvisorModemsToRemove);
        }

        private void RemoveAlertRecipientModemsReferences(List<GatewayModem> gatewayModems)
        {
            var alertRecipientsConnectedToModem = _nyssContext.AlertNotificationRecipients
                .Where(anr => gatewayModems.Contains(anr.GatewayModem))
                .ToList();

            foreach (var alertRecipient in alertRecipientsConnectedToModem)
            {
                alertRecipient.GatewayModem = null;
            }
        }
    }
}
