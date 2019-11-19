using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alert.Dto;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Project
{
    public interface IProjectService
    {
        Task<Result<ProjectResponseDto>> GetProject(int projectId);
        Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId, string userIdentityName, IEnumerable<string> roles);
        Task<Result<IEnumerable<ProjectHealthRiskResponseDto>>> GetHealthRisks(int nationalSocietyId);
        Task<Result<int>> AddProject(int nationalSocietyId, ProjectRequestDto projectRequestDto);
        Task<Result> UpdateProject(int projectId, ProjectRequestDto projectRequestDto);
        Task<Result> DeleteProject(int projectId);
        Task<Result<ProjectBasicDataResponseDto>> GetProjectBasicData(int projectId);
        Task<Result<List<ListOpenProjectsResponseDto>>> ListOpenedProjects(int nationalSocietyId);
    }

    public class ProjectService : IProjectService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ProjectService(INyssContext nyssContext, ILoggerAdapter loggerAdapter, IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<ProjectResponseDto>> GetProject(int projectId)
        {
            var project = await _nyssContext.Projects
                .Include(p => p.NationalSociety)
                .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.HealthRisk)
                        .ThenInclude(hr => hr.LanguageContents)
                .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.AlertRule)
                .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.Reports)
                .Include(p => p.AlertRecipients)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    TimeZone = p.TimeZone,
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
                    AlertRecipients = p.AlertRecipients.Select(ar => new AlertRecipientDto
                    {
                        Id = ar.Id,
                        Email = ar.EmailAddress
                    })
                })
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Error<ProjectResponseDto>(ResultKey.Project.ProjectDoesNotExist);
            }
            
            var result = Success(project);

            return result;
        }

        public async Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId, string userIdentityName, IEnumerable<string> roles)
        {
            var projectsQuery = roles.Contains(Role.Supervisor.ToString())
                ? _nyssContext.SupervisorUserProjects
                    .Where(x => x.SupervisorUser.EmailAddress == userIdentityName)
                    .Select(x => x.Project)
                : _nyssContext.Projects;

            var projects = await projectsQuery
                .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.Alerts)
                .Where(p => p.NationalSocietyId == nationalSocietyId)
                .OrderByDescending(p => p.State)
                .ThenByDescending(p => p.EndDate)
                .ThenByDescending(p => p.StartDate)
                .ThenBy(p => p.Name)
                .Select(p => new ProjectListItemResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    State = p.State,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TotalReportCount = p.ProjectHealthRisks.SelectMany(phr => phr.Reports).Count(),
                    EscalatedAlertCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Alerts
                            .Where(a => a.Status == AlertStatus.Escalated)
                        ).Count(),
                    ActiveDataCollectorCount = p.DataCollectors.Count(),
                    SupervisorCount = -1
                })
                .ToListAsync();

            var result = Success(projects);

            return result;
        }

        public async Task<Result<IEnumerable<ProjectHealthRiskResponseDto>>> GetHealthRisks(int nationalSocietyId)
        {
            var nationalSociety = await _nyssContext.NationalSocieties
                .Include(x => x.ContentLanguage)
                .FirstOrDefaultAsync(x => x.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                return Error<IEnumerable<ProjectHealthRiskResponseDto>>(ResultKey.Project.NationalSocietyDoesNotExist);
            }

            var projectHealthRisks = await _nyssContext.HealthRisks
                .Include(hr => hr.LanguageContents)
                .Select(hr => new ProjectHealthRiskResponseDto
                {
                    Id = null,
                    HealthRiskId = hr.Id,
                    HealthRiskCode = hr.HealthRiskCode,
                    HealthRiskName = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == nationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .FirstOrDefault(),
                    AlertRuleCountThreshold = hr.AlertRule.CountThreshold,
                    AlertRuleDaysThreshold = hr.AlertRule.DaysThreshold,
                    AlertRuleKilometersThreshold = hr.AlertRule.KilometersThreshold,
                    FeedbackMessage = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == nationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.FeedbackMessage)
                        .FirstOrDefault(),
                    CaseDefinition = hr.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == nationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.CaseDefinition)
                        .FirstOrDefault(),
                    ContainsReports = false
                })
                .OrderBy(hr => hr.HealthRiskCode)
                .ToListAsync();

            return Success<IEnumerable<ProjectHealthRiskResponseDto>>(projectHealthRisks);
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

                var projectToAdd = new Nyss.Data.Models.Project
                {
                    Name = projectRequestDto.Name,
                    TimeZone = projectRequestDto.TimeZone,
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
                    AlertRecipients = projectRequestDto.AlertRecipients.Select(ar => new AlertRecipient
                    {
                        EmailAddress = ar.Email
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
                    .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.Reports)
                    .Include(p => p.AlertRecipients)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (projectToUpdate == null)
                {
                    return Error(ResultKey.Project.ProjectDoesNotExist);
                }

                projectToUpdate.Name = projectRequestDto.Name;
                projectToUpdate.TimeZone = projectRequestDto.TimeZone;

                UpdateHealthRisks(projectToUpdate, projectRequestDto);

                UpdateAlertRecipients(projectToUpdate, projectRequestDto);

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.Project.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        private void UpdateHealthRisks(Nyss.Data.Models.Project projectToUpdate, ProjectRequestDto projectRequestDto)
        {
            var projectHealthRiskIdsFromDto = projectRequestDto.HealthRisks.Where(ar => ar.Id.HasValue).Select(ar => ar.Id.Value).ToList();
            var projectHealthRisksToDelete = projectToUpdate.ProjectHealthRisks.Where(ar => !projectHealthRiskIdsFromDto.Contains(ar.Id)).ToList();

            if (projectHealthRisksToDelete.Any(phr => phr.Reports.Count > 0))
            {
                throw new ResultException(ResultKey.Project.HealthRiskContainsReports); 
            }

            _nyssContext.ProjectHealthRisks.RemoveRange(projectHealthRisksToDelete);

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

        private void UpdateAlertRecipients(Nyss.Data.Models.Project projectToUpdate, ProjectRequestDto projectRequestDto)
        {
            var alertRecipientIdsFromDto = projectRequestDto.AlertRecipients.Where(ar => ar.Id.HasValue).Select(ar => ar.Id.Value).ToList();
            var alertRecipientsToDelete = projectToUpdate.AlertRecipients.Where(ar => !alertRecipientIdsFromDto.Contains(ar.Id));
            _nyssContext.AlertRecipients.RemoveRange(alertRecipientsToDelete);

            var alertRecipientsToAdd = projectRequestDto.AlertRecipients.Where(ar => ar.Id == null);
            foreach (var alertRecipient in alertRecipientsToAdd)
            {
                var alertRecipientToAdd = new AlertRecipient {EmailAddress = alertRecipient.Email};
                projectToUpdate.AlertRecipients.Add(alertRecipientToAdd);
            }

            var alertRecipientsToUpdate = projectRequestDto.AlertRecipients.Where(ar => ar.Id.HasValue);
            foreach (var alertRecipient in alertRecipientsToUpdate)
            {
                var alertRecipientToUpdate = projectToUpdate.AlertRecipients.FirstOrDefault(ar => ar.Id == alertRecipient.Id.Value);

                if (alertRecipientToUpdate != null)
                {
                    alertRecipientToUpdate.EmailAddress = alertRecipient.Email;
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
                .Include(p => p.NationalSociety).ThenInclude(n => n.Country)
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
    }
}
