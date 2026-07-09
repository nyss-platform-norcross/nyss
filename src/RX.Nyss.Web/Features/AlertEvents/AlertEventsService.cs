using System;
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

namespace RX.Nyss.Web.Features.AlertEvents;

public interface IAlertEventsService
{
    Task<Result<AlertEventsLogResponseDto>> GetLogItems(int alertId, int utcOffset);
    Task<Result> CreateAlertEventLogItem(int alertId, CreateAlertEventRequestDto alertEvent);
    Task<Result> EditAlertEventLogItem(EditAlertEventRequestDto editDto);
    Task<Result<AlertEventCreateFormDto>> GetFormData();
    Task<Result> DeleteAlertEventLogItem(int alertEventLogId);
}

public class AlertEventsService : IAlertEventsService
{
    private readonly INyssContext _nyssContext;
    private readonly IAuthorizationService _authorizationService;

    public AlertEventsService(
        INyssContext nyssContext,
        IAuthorizationService authorizationService)
    {
        _nyssContext = nyssContext ?? throw new ArgumentNullException(nameof(nyssContext));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<Result<AlertEventsLogResponseDto>> GetLogItems(int alertId, int utcOffset)
    {
        var currentUser = await _authorizationService.GetCurrentUser();

        if (currentUser == null)
        {
            return Error<AlertEventsLogResponseDto>(ResultKey.User.Common.UserNotFound);
        }

        var currentUserOrganization = await _nyssContext.UserNationalSocieties
            .Where(uns =>
                uns.UserId == currentUser.Id
                && uns.NationalSociety == _nyssContext.Alerts
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

                EscalatedBy = a.EscalatedBy == null ? null : a.EscalatedBy.EmailAddress,
                    /*a.EscalatedBy.DeletedAt.HasValue ||
                    (currentUser.Role != Role.Administrator &&
                        currentUserOrganization != (a.EscalatedBy.UserNationalSocieties ?? Enumerable.Empty<UserNationalSociety>())
                            .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                            .Organization)
                        ? (a.EscalatedBy.UserNationalSocieties ?? Enumerable.Empty<UserNationalSociety>())
                            .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                            .Organization.Name ?? string.Empty
                        : a.EscalatedBy.Name ?? string.Empty,*/

                DismissedBy = a.DismissedBy == null ? null : a.DismissedBy.EmailAddress,
                     /*
                    a.DismissedBy.DeletedAt.HasValue ||
                    (currentUser.Role != Role.Administrator &&
                        currentUserOrganization != a.DismissedBy.UserNationalSocieties
                            .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                            .Organization)
                        ? a.DismissedBy.UserNationalSocieties
                            .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                            .Organization.Name
                        : a.DismissedBy.Name,*/

                ClosedBy = a.ClosedBy == null ? null : a.ClosedBy.EmailAddress,
                     /*
                    a.ClosedBy.DeletedAt.HasValue ||
                    (currentUser.Role != Role.Administrator &&
                        currentUserOrganization != a.ClosedBy.UserNationalSocieties
                            .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                            .Organization)
                        ? a.ClosedBy.UserNationalSocieties
                            .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                            .Organization.Name
                        : a.ClosedBy.Name,*/

                HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                    .Where(lc => lc.ContentLanguage != null &&
                                 a.ProjectHealthRisk != null && a.ProjectHealthRisk.Project != null && a.ProjectHealthRisk.Project.NationalSociety != null)
                    .Select(lc => lc.Name)
                    .FirstOrDefault() ?? string.Empty,

                Reports = a.AlertReports
                    .Where(ar => ar.Report.Status == ReportStatus.Accepted
                              || ar.Report.Status == ReportStatus.Rejected
                              || ar.Report.ResetAt.HasValue)
                    .Select(ar => new
                    {
                        ar.ReportId,
                        ar.Report.AcceptedAt,
                        ar.Report.RejectedAt,
                        ar.Report.ResetAt,

                        AcceptedBy = ar.Report.AcceptedBy == null ? null : ar.Report.AcceptedBy.EmailAddress,
                             /*
                            ar.Report.AcceptedBy.DeletedAt.HasValue ||
                            (currentUser.Role != Role.Administrator &&
                                currentUserOrganization != ar.Report.AcceptedBy.UserNationalSocieties
                                    .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Organization)
                                ? ar.Report.AcceptedBy.UserNationalSocieties
                                    .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Organization.Name
                                : ar.Report.AcceptedBy.Name,*/

                        RejectedBy = ar.Report.RejectedBy == null ? null : ar.Report.RejectedBy.EmailAddress,
                             /*
                            ar.Report.RejectedBy.DeletedAt.HasValue ||
                            (currentUser.Role != Role.Administrator &&
                                currentUserOrganization != ar.Report.RejectedBy.UserNationalSocieties
                                    .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Organization)
                                ? ar.Report.RejectedBy.UserNationalSocieties
                                    .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Organization.Name
                                : ar.Report.RejectedBy.Name,*/

                        ResetBy = ar.Report.ResetBy == null ? null : ar.Report.ResetBy.EmailAddress,
                             /*
                            ar.Report.ResetBy.DeletedAt.HasValue ||
                            (currentUser.Role != Role.Administrator &&
                                currentUserOrganization != ar.Report.ResetBy.UserNationalSocieties
                                    .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Organization)
                                ? ar.Report.ResetBy.UserNationalSocieties
                                    .FirstOrDefault(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Organization.Name
                                : ar.Report.ResetBy.Name*/
                    })
                    .ToList()
            })
            .SingleAsync();

        var alertEventLogItems = await _nyssContext.AlertEventLogs
            .Where(log => log.AlertId == alertId)
            .Select(logItem => new
            {
                AlertEventLogId = logItem.Id,
                EventType = logItem.AlertEventType != null ? logItem.AlertEventType.Name ?? string.Empty : string.Empty,
                EventSubtype = logItem.AlertEventSubtype != null ? logItem.AlertEventSubtype.Name ?? string.Empty : string.Empty,
                logItem.LoggedBy,
                CreatedAt = logItem.CreatedAt.AddHours(utcOffset),
                logItem.Text
            })
            .ToListAsync();

        var list = new List<AlertEventsLogResponseDto.LogItem>
        {
            new AlertEventsLogResponseDto.LogItem
            {
                LogType = AlertEventsLogResponseDto.LogType.TriggeredAlert,
                Date = alert.CreatedAt.AddHours(utcOffset)
            }
        };

        list.AddRange(alertEventLogItems.Select(logItem => new AlertEventsLogResponseDto.LogItem
        {
            AlertEventLogId = logItem.AlertEventLogId,
            Date = logItem.CreatedAt,
            LoggedBy = logItem.LoggedBy?.Name ?? string.Empty,
            AlertEventType = logItem.EventType,
            AlertEventSubtype = logItem.EventSubtype,
            Text = logItem.Text
        }));

        if (alert.EscalatedAt.HasValue)
        {
            list.Add(new AlertEventsLogResponseDto.LogItem
            {
                LogType = AlertEventsLogResponseDto.LogType.EscalatedAlert,
                Date = alert.EscalatedAt.Value.AddHours(utcOffset),
                LoggedBy = alert.EscalatedBy ?? string.Empty
            });
        }

        if (alert.DismissedAt.HasValue)
        {
            list.Add(new AlertEventsLogResponseDto.LogItem
            {
                LogType = AlertEventsLogResponseDto.LogType.DismissedAlert,
                Date = alert.DismissedAt.Value.AddHours(utcOffset),
                LoggedBy = alert.DismissedBy ?? string.Empty
            });
        }

        if (alert.ClosedAt.HasValue)
        {
            list.Add(new AlertEventsLogResponseDto.LogItem
            {
                LogType = AlertEventsLogResponseDto.LogType.ClosedAlert,
                Date = alert.ClosedAt.Value.AddHours(utcOffset),
                LoggedBy = alert.ClosedBy ?? string.Empty,
                Metadata = new { alert.EscalatedOutcome },
                Text = alert.Comments ?? string.Empty
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
                    LoggedBy = report.AcceptedBy ?? string.Empty,
                    Metadata = new { report.ReportId }
                });
            }

            if (report.RejectedAt.HasValue)
            {
                list.Add(new AlertEventsLogResponseDto.LogItem
                {
                    LogType = AlertEventsLogResponseDto.LogType.RejectedReport,
                    Date = report.RejectedAt.Value.AddHours(utcOffset),
                    LoggedBy = report.RejectedBy ?? string.Empty,
                    Metadata = new { report.ReportId }
                });
            }

            if (report.ResetAt.HasValue)
            {
                list.Add(new AlertEventsLogResponseDto.LogItem
                {
                    LogType = AlertEventsLogResponseDto.LogType.ResetReport,
                    Date = report.ResetAt.Value.AddHours(utcOffset),
                    LoggedBy = report.ResetBy ?? string.Empty,
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

    public async Task<Result<AlertEventCreateFormDto>> GetFormData()
    {
        var eventTypes = await _nyssContext.AlertEventTypes
            .Include(a => a.AlertEventSubtypes)
            .AsSplitQuery()
            .ToListAsync();

        var types = eventTypes
            .Select(e => new AlertEventsTypeDto
            {
                Id = e.Id,
                Name = e.Name ?? string.Empty
            })
            .ToList();

        var subtypes = eventTypes
            .Where(e => e.AlertEventSubtypes != null)
            .SelectMany(e => e.AlertEventSubtypes!,
                (_, alertEventSubtype) => new AlertEventsSubtypeDto
                {
                    Id = alertEventSubtype.Id,
                    Name = alertEventSubtype.Name ?? string.Empty,
                    TypeId = alertEventSubtype.AlertEventTypeId
                })
            .ToList();

        return Success(new AlertEventCreateFormDto
        {
            EventTypes = types,
            EventSubtypes = subtypes
        });
    }

    public async Task<Result> CreateAlertEventLogItem(int alertId, CreateAlertEventRequestDto createDto)
    {
        if (createDto == null)
        {
            return Error(ResultKey.AlertEvent.AlertEventTypeNotFound);
        }

        var currentUser = await _authorizationService.GetCurrentUser();

        if (currentUser == null)
        {
            return Error(ResultKey.User.Common.UserNotFound);
        }

        var alertExists = await _nyssContext.Alerts
            .Where(a => a.Id == alertId)
            .AnyAsync();

        if (!alertExists)
        {
            return Error(ResultKey.Alert.EventLogError);
        }

        var alertEventType = await _nyssContext.AlertEventTypes
            .Where(x => x.Id == createDto.EventTypeId)
            .Include(x => x.AlertEventSubtypes)
            .SingleOrDefaultAsync();

        if (alertEventType == null)
        {
            return Error(ResultKey.AlertEvent.AlertEventTypeNotFound);
        }

        var alertEventLogItem = new AlertEventLog
        {
            AlertId = alertId,
            CreatedAt = createDto.Timestamp.DateTime,
            AlertEventTypeId = createDto.EventTypeId,
            LoggedBy = currentUser,
            Text = createDto.Text
        };

        var subtypes = alertEventType.AlertEventSubtypes
            ?? Enumerable.Empty<AlertEventSubtype>();

        if (createDto.EventSubtypeId.HasValue || subtypes.Any())
        {
            var hasValidSubtype = subtypes.Any(x => x.Id == createDto.EventSubtypeId);
            if (!hasValidSubtype)
            {
                return Error(ResultKey.AlertEvent.SubtypeMustBelongToType);
            }

            alertEventLogItem.AlertEventSubtypeId = createDto.EventSubtypeId;
        }

        await _nyssContext.AddAsync(alertEventLogItem);
        await _nyssContext.SaveChangesAsync();
        return SuccessMessage(ResultKey.AlertEvent.CreateSuccess);
    }

    public async Task<Result> EditAlertEventLogItem(EditAlertEventRequestDto editDto)
    {
        if (editDto == null)
        {
            return Error(ResultKey.AlertEvent.AlertEventNotFound);
        }

        var alertEventItem = await _nyssContext.AlertEventLogs
            .FirstOrDefaultAsync(ae => ae.Id == editDto.AlertEventLogId);

        if (alertEventItem == null)
        {
            return Error(ResultKey.AlertEvent.AlertEventNotFound);
        }

        alertEventItem.Text = editDto.Text;
        await _nyssContext.SaveChangesAsync();
        return SuccessMessage(ResultKey.AlertEvent.EditSuccess);
    }

    public async Task<Result> DeleteAlertEventLogItem(int alertEventLogId)
    {
        var alertEventItem = await _nyssContext.AlertEventLogs
            .FirstOrDefaultAsync(ae => ae.Id == alertEventLogId);

        if (alertEventItem == null)
        {
            return Error(ResultKey.AlertEvent.AlertEventNotFound);
        }

        _nyssContext.AlertEventLogs.Remove(alertEventItem);
        await _nyssContext.SaveChangesAsync();
        return SuccessMessage(ResultKey.AlertEvent.DeleteSuccess);
    }
}