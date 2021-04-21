using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Reports
{
    public interface IReportSenderService
    {
        Task<Result> SendReport(SendReportRequestDto report);
        Task<Result<SendReportFormDataDto>> GetFormData(int nationalSocietyId);
    }

    public class ReportSenderService : IReportSenderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public ReportSenderService(
            IHttpClientFactory httpClientFactory,
            ILoggerAdapter loggerAdapter,
            INyssWebConfig config,
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _httpClientFactory = httpClientFactory;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result> SendReport(SendReportRequestDto report)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var isHeadSupervisor = _authorizationService.IsCurrentUserInRole(Role.HeadSupervisor);

            var dataCollectorData = await _nyssContext.DataCollectors
                .Where(dc => dc.Id == report.DataCollectorId)
                .Select(dc => new
                {
                    DataCollectorId = dc.Id,
                    NationalSocietyId = dc.Project.NationalSocietyId
                })
                .SingleOrDefaultAsync();
            var gatewayData = await _nyssContext.GatewaySettings
                .Include(gs => gs.Modems)
                .Where(gs => gs.NationalSocietyId == dataCollectorData.NationalSocietyId)
                .Select(gs => new
                {
                    ApiKey = gs.ApiKey,
                    Modem = gs.Modems.FirstOrDefault(gm => gm.Id == report.ModemId)
                })
                .FirstOrDefaultAsync();

            if (gatewayData == null)
            {
                return Error(ResultKey.Report.NoGatewaySettingFoundForNationalSociety);
            }

            var baseUri = new Uri(_config.FuncAppBaseUrl);
            var requestUri = new Uri(baseUri, new Uri("/api/enqueueNyssReport", UriKind.Relative));
            var reportProps = new Dictionary<string, string>
            {
                { "Datacollectorid", dataCollectorData.DataCollectorId.ToString() },
                { "Timestamp", report.Timestamp },
                { "Text", report.Text },
                { "Modemno", gatewayData.Modem?.ModemId.ToString() },
                { "Headsupervisor", isHeadSupervisor ? "true" : null },
                { "UtcOffset", report.UtcOffset.ToString() },
                { "Apikey", gatewayData.ApiKey }
            };
            var content = new FormUrlEncodedContent(reportProps);

            var responseMessage = await httpClient.PostAsync(requestUri, content);

            responseMessage.EnsureSuccessStatusCode();

            return SuccessMessage(ResultKey.Report.ReportSentSuccessfully);
        }

        public async Task<Result<SendReportFormDataDto>> GetFormData(int nationalSocietyId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentUserModemId = await GetCurrentUserModemId(currentUser, nationalSocietyId);
            var gatewayModems = await _nyssContext.GatewayModems
                .Where(gm => gm.GatewaySetting.NationalSocietyId == nationalSocietyId)
                .Select(gm => new GatewayModemResponseDto
                {
                    Id = gm.Id,
                    Name = gm.Name
                })
                .ToListAsync();

            var formData = new SendReportFormDataDto
            {
                CurrentUserModemId = currentUserModemId,
                Modems = gatewayModems
            };

            return Success(formData);
        }

        private async Task<int?> GetCurrentUserModemId(User currentUser, int nationalSocietyId) =>
            currentUser.Role switch
            {
                Role.Manager => ((ManagerUser)currentUser).ModemId,
                Role.TechnicalAdvisor => await GetTechnicalAdvisorModemId(currentUser.Id, nationalSocietyId),
                Role.Supervisor => ((SupervisorUser)currentUser).ModemId,
                Role.HeadSupervisor => ((HeadSupervisorUser)currentUser).ModemId,
                _ => null
            };

        private async Task<int?> GetTechnicalAdvisorModemId(int technicalAdvisorId, int nationalSocietyId) =>
            await _nyssContext.TechnicalAdvisorUserGatewayModems
                .Where(tam => tam.TechnicalAdvisorUserId == technicalAdvisorId && tam.GatewayModem.GatewaySetting.NationalSocietyId == nationalSocietyId)
                .Select(tam => tam.GatewayModemId)
                .FirstOrDefaultAsync();
    }
}