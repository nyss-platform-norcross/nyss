﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Commands;

public class EscalateCommand : IRequest<Result>
{
    public EscalateCommand(int alertId, bool sendNotification)
    {
        AlertId = alertId;
        SendNotification = sendNotification;
    }

    private int AlertId { get; }
    private bool SendNotification { get; }

    public class Handler : IRequestHandler<EscalateCommand, Result>
    {
        private readonly INyssContext _nyssContext;
        private readonly IAlertService _alertService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;
        private readonly ISmsPublisherService _smsPublisherService;
        private readonly ISmsTextGeneratorService _smsTextGeneratorService;
        private readonly ILoggerAdapter _loggerAdapter;

        public Handler(
            INyssContext nyssContext,
            IAlertService alertService,
            IAuthorizationService authorizationService,
            IDateTimeProvider dateTimeProvider,
            IEmailPublisherService emailPublisherService,
            IEmailTextGeneratorService emailTextGeneratorService,
            ISmsPublisherService smsPublisherService,
            ISmsTextGeneratorService smsTextGeneratorService,
            ILoggerAdapter loggerAdapter
        )
        {
            _nyssContext = nyssContext;
            _alertService = alertService;
            _authorizationService = authorizationService;
            _dateTimeProvider = dateTimeProvider;
            _emailPublisherService = emailPublisherService;
            _emailTextGeneratorService = emailTextGeneratorService;
            _smsPublisherService = smsPublisherService;
            _smsTextGeneratorService = smsTextGeneratorService;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result> Handle(EscalateCommand request, CancellationToken cancellationToken)
        {
            var alertId = request.AlertId;
            var sendNotification = request.SendNotification;

            if (!await _alertService.HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.EscalateAlert.NoPermission);
            }

            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    ReportLocations = alert.AlertReports.OrderByDescending(r => r.Report.Id)
                        .Select(ar => $"{ar.Report.RawReport.Village.Name}, {ar.Report.RawReport.Village.District.Name}, {ar.Report.RawReport.Village.District.Region.Name}")
                        .Distinct(),
                    HealthRisk = alert.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Project = alert.ProjectHealthRisk.Project.Name,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    AcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted),
                    NationalSocietyId = alert.ProjectHealthRisk.Project.NationalSociety.Id
                })
                .SingleAsync();

            if (alertData.Alert.Status != AlertStatus.Open)
            {
                return Error(ResultKey.Alert.EscalateAlert.WrongStatus);
            }

            if (alertData.AcceptedReportCount < alertData.CountThreshold)
            {
                return Error(ResultKey.Alert.EscalateAlert.ThresholdNotReached);
            }

            alertData.Alert.Status = AlertStatus.Escalated;
            alertData.Alert.EscalatedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.EscalatedBy = await _authorizationService.GetCurrentUser();

            await _nyssContext.SaveChangesAsync();

            if (!sendNotification)
            {
                return Success(ResultKey.Alert.EscalateAlert.Success);
            }

            var notificationRecipients = await _alertService.GetAlertRecipients(alertId);

            try
            {
                var notificationEmails = notificationRecipients
                    .Where(nr => !string.IsNullOrEmpty(nr.Email))
                    .Select(nr => nr.Email).Distinct().ToList();
                var notificationPhoneNumbers = notificationRecipients
                    .Where(nr => !string.IsNullOrEmpty(nr.PhoneNumber))
                    .Select(nr => new SendSmsRecipient
                    {
                        PhoneNumber = nr.PhoneNumber,
                        Modem = nr.GatewayModem?.ModemId
                    })
                    .GroupBy(nr => nr.PhoneNumber)
                    .Select(g => g.First())
                    .ToList();

                await SendNotificationEmails(
                    alertData.LanguageCode,
                    notificationEmails,
                    alertData.Project,
                    alertData.HealthRisk,
                    alertData.ReportLocations);

                await SendNotificationSmses(
                    alertData.NationalSocietyId,
                    alertData.LanguageCode,
                    notificationPhoneNumbers,
                    alertData.Project,
                    alertData.HealthRisk,
                    alertData.ReportLocations.First());

                alertData.Alert.RecipientsNotifiedAt = _dateTimeProvider.UtcNow;

                await _nyssContext.SaveChangesAsync();

                return Success(ResultKey.Alert.EscalateAlert.Success);
            }
            catch (ResultException exception)
            {
                return Success(exception.Result.Message.Key);
            }
        }

        public async Task SendNotificationEmails(string languageCode, List<string> notificationEmails, string project, string healthRisk, IEnumerable<string> reportLocations)
        {
            try
            {
                var (subject, body) = await _emailTextGeneratorService.GenerateEscalatedAlertEmail(languageCode);
                var reportLocationsList = reportLocations.Aggregate("", (a, c) => $"{a}<li>{c}</li>\n");

                body = body
                    .Replace("{{project}}", project)
                    .Replace("{{healthRisk}}", healthRisk)
                    .Replace("{{reportLocations}}", reportLocationsList);

                foreach (var email in notificationEmails)
                {
                    await _emailPublisherService.SendEmail((email, email), subject, body);
                }
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"Failed to send escalation notification emails for project {project} with health risk {healthRisk}");
                throw new ResultException(ResultKey.Alert.EscalateAlert.EmailNotificationFailed);
            }
        }

        public async Task SendNotificationSmses(int nationalSocietyId, string languageCode, List<SendSmsRecipient> notificationRecipients, string project, string healthRisk,
            string lastReportLocation)
        {
            try
            {
                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Where(gs => gs.NationalSocietyId == nationalSocietyId).FirstOrDefaultAsync();

                if (gatewaySetting == null)
                {
                    throw new ArgumentException("SmsGateway not found!");
                }

                var text = await _smsTextGeneratorService.GenerateEscalatedAlertSms(languageCode);
                text = text
                    .Replace("{{project}}", project)
                    .Replace("{{healthRisk}}", healthRisk)
                    .Replace("{{lastReportLocation}}", lastReportLocation);

                if (!string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
                {
                    await _smsPublisherService.SendSms(gatewaySetting.IotHubDeviceName, notificationRecipients, text);
                }
                else
                {
                    await Task.WhenAll(notificationRecipients
                        .Select(sms => _emailPublisherService.SendEmail((gatewaySetting.EmailAddress, gatewaySetting.EmailAddress), sms.PhoneNumber, text, true)));
                }
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"Failed to send escalation notification SMSes for project {project} with health risk {healthRisk}");
                throw new ResultException(ResultKey.Alert.EscalateAlert.SmsNotificationFailed);
            }
        }
    }
}
