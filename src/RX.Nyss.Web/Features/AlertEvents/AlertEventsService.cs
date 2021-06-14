using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.AlertEvents.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.AlertEvents
{
    public interface IAlertEventsService
    {
        Task<Result<AlertEventsLogResponseDto>> GetLogItems(int alertId, int utcOffset);
        Task<Result> CreateAlertEventLogItem(int alertId, CreateAlertEventRequestDto alertEvent);
        Task<Result> EditAlertEventLogItem(EditAlertEventRequestDto editDto);
        Task<Result<AlertEventCreateFormDto>> GetFormData();
    }

    public class AlertEventsService : IAlertEventsService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public AlertEventsService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<AlertEventsLogResponseDto>> GetLogItems(int alertId, int utcOffset)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentUserOrganization = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id && uns.NationalSociety == _nyssContext.Alerts
                    .Where(a => a.Id == alertId)
                    .Select(a => a.ProjectHealthRisk.Project.NationalSociety)
                    .Single())
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            var alert = await _nyssContext.Alerts
                .IgnoreQueryFilters()
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    a.CreatedAt,
                    a.EscalatedAt,
                    a.DismissedAt,
                    a.ClosedAt,
                    a.EscalatedOutcome,
                    a.Comments,
                    EscalatedBy = a.EscalatedBy.DeletedAt.HasValue ||
                        (currentUser.Role != Role.Administrator &&
                            currentUserOrganization != a.EscalatedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                            ? a.EscalatedBy.UserNationalSocieties.Single(escalatedUns => escalatedUns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                            : a.EscalatedBy.Name,
                    DismissedBy = a.DismissedBy.DeletedAt.HasValue ||
                        (currentUser.Role != Role.Administrator &&
                            currentUserOrganization != a.DismissedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                            ? a.DismissedBy.UserNationalSocieties.Single(dismissedUns => dismissedUns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                            : a.DismissedBy.Name,
                    ClosedBy = a.ClosedBy.DeletedAt.HasValue ||
                        (currentUser.Role != Role.Administrator &&
                            currentUserOrganization != a.ClosedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                            ? a.ClosedBy.UserNationalSocieties.Single(closedUns => closedUns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                            : a.ClosedBy.Name,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Reports = a.AlertReports
                        .Where(ar => ar.Report.Status == ReportStatus.Accepted || ar.Report.Status == ReportStatus.Rejected || ar.Report.ResetAt.HasValue)
                        .Select(ar => new
                        {
                            ar.ReportId,
                            ar.Report.AcceptedAt,
                            ar.Report.RejectedAt,
                            ar.Report.ResetAt,
                            AcceptedBy = ar.Report.AcceptedBy.DeletedAt.HasValue ||
                                (currentUser.Role != Role.Administrator &&
                                    currentUserOrganization != ar.Report.AcceptedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                        .Organization)
                                    ? ar.Report.AcceptedBy.UserNationalSocieties.Single(acceptedUns => acceptedUns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                                    : ar.Report.AcceptedBy.Name,
                            RejectedBy = ar.Report.RejectedBy.DeletedAt.HasValue ||
                                (currentUser.Role != Role.Administrator &&
                                    currentUserOrganization != ar.Report.RejectedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                        .Organization)
                                    ? ar.Report.RejectedBy.UserNationalSocieties.Single(rejectedUns => rejectedUns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                                    : ar.Report.RejectedBy.Name,
                            ResetBy = ar.Report.ResetBy.DeletedAt.HasValue ||
                                (currentUser.Role != Role.Administrator &&
                                    currentUserOrganization != ar.Report.ResetBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                                    ? ar.Report.ResetBy.UserNationalSocieties.Single(resetUns => resetUns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                                    : ar.Report.ResetBy.Name
                        })
                        .ToList()
                })
                .SingleAsync();

            var alertEventLogItems = _nyssContext.AlertEventLogs
                .Where(log => log.AlertId == alertId)
                .Select(logItem => new
                {
                    EventType = logItem.AlertEventType.Name,
                    EventSubtype = logItem.AlertEventSubtype.Name,
                    logItem.LoggedBy,
                    CreatedAt = logItem.CreatedAt.AddHours(utcOffset),
                    logItem.Textfield
                })
                .ToList();

            var list = new List<AlertEventsLogResponseDto.LogItem>
            {
                new AlertEventsLogResponseDto.LogItem
                {
                    LogType = AlertEventsLogResponseDto.LogType.TriggeredAlert,
                    Date =  alert.CreatedAt.AddHours(utcOffset)
                }
            };

            list.AddRange(alertEventLogItems.Select(logItem => new AlertEventsLogResponseDto.LogItem()
            {
                Date = logItem.CreatedAt,
                LoggedBy = logItem.LoggedBy.Name,
                AlertEventType = logItem.EventType,
                AlertEventSubtype = logItem.EventSubtype,
                Text = logItem.Textfield
            }));

            if (alert.EscalatedAt.HasValue)
            {
                list.Add(new AlertEventsLogResponseDto.LogItem
                {
                    LogType = AlertEventsLogResponseDto.LogType.EscalatedAlert,
                    Date = alert.EscalatedAt.Value.AddHours(utcOffset),
                    LoggedBy = alert.EscalatedBy
                });
            }

            if (alert.DismissedAt.HasValue)
            {
                list.Add(new AlertEventsLogResponseDto.LogItem
                {
                    LogType = AlertEventsLogResponseDto.LogType.DismissedAlert,
                    Date = alert.DismissedAt.Value.AddHours(utcOffset),
                    LoggedBy = alert.DismissedBy
                });
            }

            if (alert.ClosedAt.HasValue)
            {
                list.Add(new AlertEventsLogResponseDto.LogItem
                {
                    LogType = AlertEventsLogResponseDto.LogType.ClosedAlert,
                    Date = alert.ClosedAt.Value.AddHours(utcOffset),
                    LoggedBy = alert.ClosedBy,
                    Metadata = new { alert.EscalatedOutcome },
                    Text = alert.Comments
                });
            }

            foreach (var report in alert.Reports)
            {
                if (report.AcceptedAt.HasValue)
                {
                    list.Add(new AlertEventsLogResponseDto.LogItem
                    {
                        LogType = AlertEventsLogResponseDto.LogType.AcceptedReport,
                        Date = report.AcceptedAt.Value.AddHours(utcOffset),
                        LoggedBy = report.AcceptedBy,
                        Metadata = new { report.ReportId }
                    });
                }

                if (report.RejectedAt.HasValue)
                {
                    list.Add(new AlertEventsLogResponseDto.LogItem
                    {
                        LogType = AlertEventsLogResponseDto.LogType.RejectedReport,
                        Date = report.RejectedAt.Value.AddHours(utcOffset),
                        LoggedBy = report.RejectedBy,
                        Metadata = new { report.ReportId }
                    });
                }

                if (report.ResetAt.HasValue)
                {
                    list.Add(new AlertEventsLogResponseDto.LogItem
                    {
                        LogType = AlertEventsLogResponseDto.LogType.ResetReport,
                        Date = report.ResetAt.Value.AddHours(utcOffset),
                        LoggedBy = report.ResetBy,
                        Metadata = new { report.ReportId }
                    });
                }
            }

            return Success(new AlertEventsLogResponseDto
            {
                HealthRisk = alert.HealthRisk,
                CreatedAt = alert.CreatedAt.AddHours(utcOffset),
                LogItems = list.OrderBy(x => x.Date).ToList()
            });
        }

        public async Task<Result> CreateAlertEventLogItem(int alertId, CreateAlertEventRequestDto createDto)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => a.Id)
                .SingleAsync();

            var alertEventLogItem = new AlertEventLog()
            {
                AlertId = alert,
                CreatedAt = createDto.Timestamp.ToUniversalTime(),
                AlertEventTypeId = createDto.EventTypeId,
                AlertEventSubtypeId = createDto.EventSubtypeId,
                LoggedBy = currentUser,
                Textfield = createDto.Text
            };

            await _nyssContext.AddAsync(alertEventLogItem);
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.AlertEvent.CreateSuccess);
        }

        public async Task<Result<AlertEventCreateFormDto>> GetFormData()
        {
            var eventTypes = await _nyssContext.AlertEventTypes
                .Include((a => a.AlertEventSubtype))
                .ToListAsync();

            var types = eventTypes.Select(e => new AlertEventsTypeDto
                {
                Id = e.Id,
                Name = e.Name
            });

            var subtypes = eventTypes
                .SelectMany(alertEventType =>
                    alertEventType.AlertEventSubtype,
                (alertEventType, alertEventSubtype) => new AlertEventsSubtypeDto
                        {
                                Id = alertEventSubtype.Id,
                                Name = alertEventSubtype.Name,
                                TypeId = alertEventSubtype.AlertEventTypeId
                            });

            return Success(new AlertEventCreateFormDto
            {
                EventTypes = types,
                EventSubtypes = subtypes,
            });
        }
    }
}