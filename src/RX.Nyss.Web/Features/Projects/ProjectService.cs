using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Projects.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Projects
{
    public interface IProjectService
    {
        Task<Result<ProjectResponseDto>> GetProject(int projectId);
        Task<Result<List<ProjectListItemResponseDto>>> ListProjects(int nationalSocietyId);
        Task<Result<int>> AddProject(int nationalSocietyId, ProjectRequestDto projectRequestDto);
        Task<Result> UpdateProject(int projectId, ProjectRequestDto projectRequestDto);
        Task<Result> DeleteProject(int projectId);
        Task<Result<ProjectBasicDataResponseDto>> GetProjectBasicData(int projectId);
        Task<Result<List<ListOpenProjectsResponseDto>>> ListOpenedProjects(int nationalSocietyId);
        Task<Result<ProjectFormDataResponseDto>> GetFormData(int nationalSocietyId);
        Task<IEnumerable<HealthRiskDto>> GetProjectHealthRiskNames(int projectId);
        Task<IEnumerable<int>> GetSupervisorProjectIds(string supervisorIdentityName);
    }

    public class ProjectService : IProjectService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;

        public ProjectService(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
        }

        public async Task<Result<ProjectResponseDto>> GetProject(int projectId)
        {
            var project = await _nyssContext.Projects
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    TimeZoneId = p.TimeZone,
                    State = p.State,
                    ProjectHealthRisks = p.ProjectHealthRisks.Select(phr => new ProjectHealthRiskResponseDto
                    {
                        Id = phr.Id,
                        HealthRiskId = phr.HealthRiskId,
                        HealthRiskCode = phr.HealthRisk.HealthRiskCode,
                        HealthRiskName = phr.HealthRisk.LanguageContents
                            .Where(lc => lc.ContentLanguage.Id == p.NationalSociety.ContentLanguage.Id)
                            .Select(lc => lc.Name)
                            .FirstOrDefault(),
                        AlertRuleCountThreshold = phr.AlertRule.CountThreshold,
                        AlertRuleDaysThreshold = phr.AlertRule.DaysThreshold,
                        AlertRuleKilometersThreshold = phr.AlertRule.KilometersThreshold,
                        FeedbackMessage = phr.FeedbackMessage,
                        CaseDefinition = phr.CaseDefinition,
                        ContainsReports = phr.Reports.Any()
                    }),
                    EmailAlertRecipients = p.EmailAlertRecipients.Select(ear => new EmailAlertRecipientDto
                    {
                        Id = ear.Id,
                        Email = ear.EmailAddress
                    }),
                    SmsAlertRecipients = p.SmsAlertRecipients.Select(sar => new SmsAlertRecipientDto
                    {
                        Id = sar.Id,
                        PhoneNumber = sar.PhoneNumber
                    }),
                    ContentLanguageId = p.NationalSociety.ContentLanguage.Id,
                })
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Error<ProjectResponseDto>(ResultKey.Project.ProjectDoesNotExist);
            }

            project.FormData = await GetFormDataDto(project.ContentLanguageId);

            var result = Success(project);

            return result;
        }

        public async Task<IEnumerable<HealthRiskDto>> GetProjectHealthRiskNames(int projectId) =>
            await _nyssContext.ProjectHealthRisks
                .Where(ph => ph.Project.Id == projectId && ph.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                .Select(ph => new HealthRiskDto
                {
                    Id = ph.HealthRiskId,
                    Name = ph.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == ph.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

        public async Task<Result<List<ProjectListItemResponseDto>>> ListProjects(int nationalSocietyId)
        {
            var userIdentityName = _authorizationService.GetCurrentUserName();

            var projectsQuery = _authorizationService.IsCurrentUserInRole(Role.Supervisor)
                ? _nyssContext.SupervisorUserProjects
                    .Where(x => x.SupervisorUser.EmailAddress == userIdentityName)
                    .Select(x => x.Project)
                : _nyssContext.Projects;

            var projects = await projectsQuery
                .Where(p => p.NationalSocietyId == nationalSocietyId)
                .OrderByDescending(p => p.State)
                .ThenByDescending(p => p.EndDate)
                .ThenByDescending(p => p.StartDate)
                .ThenBy(p => p.Name)
                .Select(p => new ProjectListItemResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TotalReportCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Reports)
                        .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity && !r.IsTraining && !r.MarkedAsError)
                        .Sum(r => r.ReportedCaseCount),
                    EscalatedAlertCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Alerts
                            .Where(a => a.Status == AlertStatus.Escalated)
                        ).Count(),
                    TotalDataCollectorCount = p.DataCollectors.Count(dc => dc.Name != Anonymization.Text && dc.DeletedAt == null),
                    SupervisorCount = p.SupervisorUserProjects.Count
                })
                .ToListAsync();

            var result = Success(projects);

            return result;
        }

        public async Task<Result<int>> AddProject(int nationalSocietyId, ProjectRequestDto projectRequestDto)
        {
            try
            {
                var nationalSocietyExists =
                    await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId);

                if (!nationalSocietyExists)
                {
                    return Error<int>(ResultKey.Project.NationalSocietyDoesNotExist);
                }

                var healthRiskIdsInDatabase = await _nyssContext.HealthRisks.Select(hr => hr.Id).ToListAsync();
                var healthRiskIdsToAttach = projectRequestDto.HealthRisks.Select(hr => hr.HealthRiskId).ToList();

                if (!healthRiskIdsToAttach.All(healthRiskId => healthRiskIdsInDatabase.Contains(healthRiskId)))
                {
                    return Error<int>(ResultKey.Project.HealthRiskDoesNotExist);
                }

                var projectToAdd = new Project
                {
                    Name = projectRequestDto.Name,
                    TimeZone = projectRequestDto.TimeZoneId,
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open,
                    StartDate = _dateTimeProvider.UtcNow,
                    EndDate = null,
                    ProjectHealthRisks = projectRequestDto.HealthRisks.Select(phr => new ProjectHealthRisk
                    {
                        FeedbackMessage = phr.FeedbackMessage,
                        CaseDefinition = phr.CaseDefinition,
                        HealthRiskId = phr.HealthRiskId,
                        AlertRule = new AlertRule
                        {
                            //ToDo: make CountThreshold nullable or change validation
                            CountThreshold = phr.AlertRuleCountThreshold ?? 0,
                            DaysThreshold = phr.AlertRuleDaysThreshold,
                            KilometersThreshold = phr.AlertRuleKilometersThreshold
                        }
                    }).ToList(),
                    EmailAlertRecipients = projectRequestDto.EmailAlertRecipients.Select(ar => new EmailAlertRecipient
                    {
                        EmailAddress = ar.Email
                    }).ToList(),
                    SmsAlertRecipients = projectRequestDto.SmsAlertRecipients.Select(ar => new SmsAlertRecipient
                    {
                        PhoneNumber = ar.PhoneNumber
                    }).ToList()
                };

                await _nyssContext.Projects.AddAsync(projectToAdd);
                await _nyssContext.SaveChangesAsync();

                return Success(projectToAdd.Id, ResultKey.Project.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> UpdateProject(int projectId, ProjectRequestDto projectRequestDto)
        {
            try
            {
                var projectToUpdate = await _nyssContext.Projects
                    .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.AlertRule)
                    .Include(p => p.EmailAlertRecipients)
                    .Include(p => p.SmsAlertRecipients)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (projectToUpdate == null)
                {
                    return Error(ResultKey.Project.ProjectDoesNotExist);
                }

                projectToUpdate.Name = projectRequestDto.Name;
                projectToUpdate.TimeZone = projectRequestDto.TimeZoneId;

                await UpdateHealthRisks(projectToUpdate, projectRequestDto);

                UpdateEmailAlertRecipients(projectToUpdate, projectRequestDto);
                UpdateSmsAlertRecipients(projectToUpdate, projectRequestDto);

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.Project.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        private async Task UpdateHealthRisks(Project projectToUpdate, ProjectRequestDto projectRequestDto)
        {
            var projectHealthRiskIdsFromDto = projectRequestDto.HealthRisks.Where(ar => ar.Id.HasValue).Select(ar => ar.Id.Value).ToList();

            var projectHealthRisksToDelete = await _nyssContext.ProjectHealthRisks
                .Where(phr => phr.Project.Id == projectToUpdate.Id && !projectHealthRiskIdsFromDto.Contains(phr.Id))
                .Select(phr => new
                {
                    ProjectHealthRisk = phr,
                    ReportCount = phr.Reports.Count
                })
                .ToListAsync();

            if (projectHealthRisksToDelete.Any(phr => phr.ReportCount > 0))
            {
                throw new ResultException(ResultKey.Project.HealthRiskContainsReports);
            }

            _nyssContext.ProjectHealthRisks.RemoveRange(projectHealthRisksToDelete.Select(phr => phr.ProjectHealthRisk));

            var projectHealthRisksToAdd = projectRequestDto.HealthRisks.Where(ar => ar.Id == null);
            foreach (var projectHealthRisk in projectHealthRisksToAdd)
            {
                var projectHealthRiskToAdd = new ProjectHealthRisk
                {
                    HealthRiskId = projectHealthRisk.HealthRiskId,
                    CaseDefinition = projectHealthRisk.CaseDefinition,
                    FeedbackMessage = projectHealthRisk.FeedbackMessage,
                    AlertRule = new AlertRule
                    {
                        //ToDo: make CountThreshold nullable or change validation
                        CountThreshold = projectHealthRisk.AlertRuleCountThreshold ?? 0,
                        DaysThreshold = projectHealthRisk.AlertRuleDaysThreshold,
                        KilometersThreshold = projectHealthRisk.AlertRuleKilometersThreshold
                    }
                };

                projectToUpdate.ProjectHealthRisks.Add(projectHealthRiskToAdd);
            }

            var projectHealthRisksToUpdate = projectRequestDto.HealthRisks.Where(ar => ar.Id.HasValue);
            foreach (var projectHealthRisk in projectHealthRisksToUpdate)
            {
                var projectHealthRiskToUpdate = projectToUpdate.ProjectHealthRisks.FirstOrDefault(ar => ar.Id == projectHealthRisk.Id.Value);

                if (projectHealthRiskToUpdate != null)
                {
                    projectHealthRiskToUpdate.CaseDefinition = projectHealthRisk.CaseDefinition;
                    projectHealthRiskToUpdate.FeedbackMessage = projectHealthRisk.FeedbackMessage;
                    //ToDo: make CountThreshold nullable or change validation
                    projectHealthRiskToUpdate.AlertRule.CountThreshold = projectHealthRisk.AlertRuleCountThreshold ?? 0;
                    projectHealthRiskToUpdate.AlertRule.DaysThreshold = projectHealthRisk.AlertRuleDaysThreshold;
                    projectHealthRiskToUpdate.AlertRule.KilometersThreshold = projectHealthRisk.AlertRuleKilometersThreshold;
                }
            }
        }

        private void UpdateEmailAlertRecipients(Project projectToUpdate, ProjectRequestDto projectRequestDto)
        {
            var alertRecipientIdsFromDto = projectRequestDto.EmailAlertRecipients.Where(ar => ar.Id.HasValue).Select(ar => ar.Id.Value).ToList();
            var alertRecipientsToDelete = projectToUpdate.EmailAlertRecipients.Where(ar => !alertRecipientIdsFromDto.Contains(ar.Id));
            _nyssContext.EmailAlertRecipients.RemoveRange(alertRecipientsToDelete);

            var alertRecipientsToAdd = projectRequestDto.EmailAlertRecipients.Where(ar => ar.Id == null);
            foreach (var alertRecipient in alertRecipientsToAdd)
            {
                var alertRecipientToAdd = new EmailAlertRecipient { EmailAddress = alertRecipient.Email };
                projectToUpdate.EmailAlertRecipients.Add(alertRecipientToAdd);
            }

            var alertRecipientsToUpdate = projectRequestDto.EmailAlertRecipients.Where(ar => ar.Id.HasValue);
            foreach (var alertRecipient in alertRecipientsToUpdate)
            {
                var alertRecipientToUpdate = projectToUpdate.EmailAlertRecipients.FirstOrDefault(ar => ar.Id == alertRecipient.Id.Value);

                if (alertRecipientToUpdate != null)
                {
                    alertRecipientToUpdate.EmailAddress = alertRecipient.Email;
                }
            }
        }

        private void UpdateSmsAlertRecipients(Project projectToUpdate, ProjectRequestDto projectRequestDto)
        {
            var alertRecipientIdsFromDto = projectRequestDto.SmsAlertRecipients.Where(ar => ar.Id.HasValue).Select(ar => ar.Id.Value).ToList();
            var alertRecipientsToDelete = projectToUpdate.SmsAlertRecipients.Where(ar => !alertRecipientIdsFromDto.Contains(ar.Id));
            _nyssContext.SmsAlertRecipients.RemoveRange(alertRecipientsToDelete);

            var alertRecipientsToAdd = projectRequestDto.SmsAlertRecipients.Where(ar => ar.Id == null);
            foreach (var alertRecipient in alertRecipientsToAdd)
            {
                var alertRecipientToAdd = new SmsAlertRecipient { PhoneNumber = alertRecipient.PhoneNumber };
                projectToUpdate.SmsAlertRecipients.Add(alertRecipientToAdd);
            }

            var alertRecipientsToUpdate = projectRequestDto.SmsAlertRecipients.Where(ar => ar.Id.HasValue);
            foreach (var alertRecipient in alertRecipientsToUpdate)
            {
                var alertRecipientToUpdate = projectToUpdate.SmsAlertRecipients.FirstOrDefault(ar => ar.Id == alertRecipient.Id.Value);

                if (alertRecipientToUpdate != null)
                {
                    alertRecipientToUpdate.PhoneNumber = alertRecipient.PhoneNumber;
                }
            }
        }

        public async Task<Result> DeleteProject(int projectId)
        {
            try
            {
                var projectToDelete = await _nyssContext.Projects.FindAsync(projectId);

                if (projectToDelete == null)
                {
                    return Error(ResultKey.Project.ProjectDoesNotExist);
                }

                _nyssContext.Projects.Remove(projectToDelete);
                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.Project.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result<ProjectBasicDataResponseDto>> GetProjectBasicData(int projectId)
        {
            var project = await _nyssContext.Projects
                .Select(dc => new ProjectBasicDataResponseDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    NationalSociety = new ProjectBasicDataResponseDto.NationalSocietyIdDto
                    {
                        Id = dc.NationalSociety.Id,
                        Name = dc.NationalSociety.Name,
                        CountryName = dc.NationalSociety.Country.Name,
                    }
                })
                .SingleAsync(p => p.Id == projectId);

            return Success(project);
        }

        public async Task<Result<List<ListOpenProjectsResponseDto>>> ListOpenedProjects(int nationalSocietyId)
        {
            var projects = await _nyssContext.Projects
                .Where(p => p.NationalSociety.Id == nationalSocietyId)
                .Where(p => p.State == ProjectState.Open)
                .Select(p => new ListOpenProjectsResponseDto()
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return Success(projects);
        }

        public async Task<Result<ProjectFormDataResponseDto>> GetFormData(int nationalSocietyId)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties
                    .Include(x => x.ContentLanguage)
                    .FirstOrDefaultAsync(x => x.Id == nationalSocietyId);

                if (nationalSociety == null)
                {
                    throw new ResultException(ResultKey.Project.NationalSocietyDoesNotExist);
                }

                var result = await GetFormDataDto(nationalSociety.ContentLanguage.Id);
                return Success(result);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<ProjectFormDataResponseDto>();
            }
        }

        public async Task<IEnumerable<int>> GetSupervisorProjectIds(string supervisorIdentityName) =>
            await _nyssContext.Users
                .OfType<SupervisorUser>()
                .Where(u => u.EmailAddress == supervisorIdentityName)
                .SelectMany(u => u.SupervisorUserProjects.Select(sup => sup.ProjectId))
                .ToListAsync();
        
        private async Task<ProjectFormDataResponseDto> GetFormDataDto(int contentLanguageId)
        {
            var projectHealthRisks = await _nyssContext.HealthRisks
                .Include(hr => hr.LanguageContents)
                .Select(hr => new ProjectHealthRiskResponseDto
                {
                    Id = null,
                    HealthRiskId = hr.Id,
                    HealthRiskCode = hr.HealthRiskCode,
                    HealthRiskName = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == contentLanguageId)
                        .Select(lc => lc.Name)
                        .FirstOrDefault(),
                    AlertRuleCountThreshold = hr.AlertRule.CountThreshold,
                    AlertRuleDaysThreshold = hr.AlertRule.DaysThreshold,
                    AlertRuleKilometersThreshold = hr.AlertRule.KilometersThreshold,
                    FeedbackMessage = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == contentLanguageId)
                        .Select(lc => lc.FeedbackMessage)
                        .FirstOrDefault(),
                    CaseDefinition = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == contentLanguageId)
                        .Select(lc => lc.CaseDefinition)
                        .FirstOrDefault(),
                    ContainsReports = false
                })
                .OrderBy(hr => hr.HealthRiskCode)
                .ToListAsync();

            var timeZones = GetTimeZones();

            return new ProjectFormDataResponseDto { TimeZones = timeZones, HealthRisks = projectHealthRisks };
        }

        private IEnumerable<TimeZoneResponseDto> GetTimeZones()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new TimeZoneResponseDto
                {
                    Id = tz.Id,
                    DisplayName = tz.DisplayName
                });
            return timeZones;
        }
    }
}
