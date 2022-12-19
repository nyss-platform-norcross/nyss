using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors;
using RX.Nyss.Web.Features.Projects.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Projects
{
    public interface IProjectService
    {
        Task<Result<ProjectResponseDto>> Get(int projectId);
        Task<Result<List<ProjectListItemResponseDto>>> List(int nationalSocietyId, int utcOffset);
        Task<Result<int>> Create(int nationalSocietyId, CreateProjectRequestDto dto);
        Task<Result> Edit(int projectId, EditProjectRequestDto dto);
        Task<Result> Close(int projectId);
        Task<Result<ProjectFormDataResponseDto>> GetFormData(int nationalSocietyId);
        Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int projectId, IEnumerable<HealthRiskType> healthRiskTypes);
    }

    public class ProjectService : IProjectService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataCollectorService _dataCollectorService;

        public ProjectService(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService,
            IDataCollectorService dataCollectorService)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
            _dataCollectorService = dataCollectorService;
        }

        public async Task<Result<ProjectResponseDto>> Get(int projectId)
        {
            var project = await _nyssContext.Projects
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    NationalSocietyId = p.NationalSocietyId,
                    Name = p.Name,
                    AllowMultipleOrganizations = p.AllowMultipleOrganizations,
                    State = p.State,
                    ProjectHealthRisks = p.ProjectHealthRisks.OrderBy(phr => phr.HealthRisk.HealthRiskCode)
                        .Select(phr => new ProjectHealthRiskResponseDto
                        {
                            Id = phr.Id,
                            HealthRiskId = phr.HealthRiskId,
                            HealthRiskType = phr.HealthRisk.HealthRiskType,
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
                    ContentLanguageId = p.NationalSociety.ContentLanguage.Id,
                    HasCoordinator = p.NationalSociety.NationalSocietyUsers.Any(nsu => nsu.User.Role == Role.Coordinator)
                })
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Error<ProjectResponseDto>(ResultKey.Project.ProjectDoesNotExist);
            }

            project.FormData = await GetFormDataDto(project.NationalSocietyId, project.ContentLanguageId);

            var result = Success(project);

            return result;
        }

        public async Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int projectId, IEnumerable<HealthRiskType> healthRiskTypes) =>
            await _nyssContext.ProjectHealthRisks
                .Where(ph => ph.Project.Id == projectId && healthRiskTypes.Contains(ph.HealthRisk.HealthRiskType))
                .Select(ph => new HealthRiskDto
                {
                    Id = ph.HealthRiskId,
                    Name = ph.HealthRisk.LanguageContents
                        //.Where(lc => lc.ContentLanguage.Id == ph.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .FirstOrDefault()
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

        public async Task<Result<List<ProjectListItemResponseDto>>> List(int nationalSocietyId, int utcOffset)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var projectsQuery = _authorizationService.IsCurrentUserInAnyRole(Role.HeadSupervisor, Role.Supervisor)
                ? GetProjectsForSupervisorOrHeadSupervisor(currentUser)
                : _nyssContext.Projects;

            var nationalSocietyData = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    CurrentUserOrganizationId = ns.NationalSocietyUsers
                        .Where(uns => uns.User == currentUser)
                        .Select(uns => uns.OrganizationId)
                        .SingleOrDefault(),
                    HasCoordinator = ns.NationalSocietyUsers
                        .Any(uns => uns.User.Role == Role.Coordinator)
                })
                .SingleAsync();

            var query = projectsQuery
                .Where(p => p.NationalSocietyId == nationalSocietyId);

            if (nationalSocietyData.HasCoordinator && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator, Role.DataConsumer))
            {
                query = query
                    .Where(p => p.ProjectOrganizations.Any(po => po.OrganizationId == nationalSocietyData.CurrentUserOrganizationId));
            }

            var projects = await query
                .OrderByDescending(p => p.State)
                .ThenByDescending(p => p.EndDate)
                .ThenByDescending(p => p.StartDate)
                .ThenBy(p => p.Name)
                .Select(p => new ProjectListItemResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StartDate = p.StartDate.AddHours(utcOffset),
                    EndDate = p.EndDate.HasValue ? p.EndDate.Value.AddHours(utcOffset) : null,
                    IsClosed = p.State == ProjectState.Closed,
                    TotalReportCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Reports)
                        .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity && !r.IsTraining)
                        .Sum(r => r.ReportedCaseCount),
                    EscalatedAlertCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Alerts.Where(a => a.EscalatedAt.HasValue)).Count(),
                    TotalDataCollectorCount = p.DataCollectors.Count(dc => dc.Name != Anonymization.Text && dc.DeletedAt == null),
                    SupervisorCount = p.SupervisorUserProjects.Count
                })
                .ToListAsync();

            var result = Success(projects);

            return result;
        }

        public async Task<Result<int>> Create(int nationalSocietyId, CreateProjectRequestDto dto)
        {
            try
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                var data = await _nyssContext.NationalSocieties
                    .Where(ns => ns.Id == nationalSocietyId)
                    .Select(ns => new
                    {
                        ns.IsArchived,
                        OrganizationsCount = ns.Organizations.Count,
                        FirstOrganizationId = (int?)ns.Organizations
                            .Select(o => o.Id)
                            .FirstOrDefault(),
                        UserOrganizationId = ns.NationalSocietyUsers
                            .Where(nsu => nsu.User == currentUser && nsu.NationalSocietyId == nationalSocietyId)
                            .Select(nsu => nsu.OrganizationId)
                            .FirstOrDefault()
                    })
                    .SingleOrDefaultAsync();

                if (data == null)
                {
                    return Error<int>(ResultKey.Project.NationalSocietyDoesNotExist);
                }

                if (data.IsArchived)
                {
                    return Error<int>(ResultKey.Project.CannotAddProjectInArchivedNationalSociety);
                }

                var healthRiskIdsInDatabase = await _nyssContext.HealthRisks.Select(hr => hr.Id).ToListAsync();
                var healthRiskIdsToAttach = dto.HealthRisks.Select(hr => hr.HealthRiskId).ToList();

                if (!healthRiskIdsToAttach.All(healthRiskId => healthRiskIdsInDatabase.Contains(healthRiskId)))
                {
                    return Error<int>(ResultKey.Project.HealthRiskDoesNotExist);
                }

                var organizationId = dto.OrganizationId
                    ?? (data.OrganizationsCount == 1
                        ? data.FirstOrganizationId
                        : data.UserOrganizationId);

                var projectToAdd = new Project
                {
                    Name = dto.Name,
                    AllowMultipleOrganizations = dto.AllowMultipleOrganizations,
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open,
                    StartDate = _dateTimeProvider.UtcNow,
                    EndDate = null,

                    ProjectHealthRisks = dto.HealthRisks.Select(phr => new ProjectHealthRisk
                    {
                        FeedbackMessage = phr.FeedbackMessage,
                        CaseDefinition = phr.CaseDefinition,
                        HealthRiskId = phr.HealthRiskId,
                        AlertRule = new AlertRule
                        {
                            CountThreshold = phr.AlertRuleCountThreshold ?? 0,
                            DaysThreshold = phr.AlertRuleDaysThreshold,
                            KilometersThreshold = phr.AlertRuleKilometersThreshold
                        }
                    }).ToList(),

                    ProjectOrganizations = organizationId.HasValue
                        ? new List<ProjectOrganization> { new ProjectOrganization { OrganizationId = organizationId.Value } }
                        : new List<ProjectOrganization>()
                };

                await _nyssContext.Projects.AddAsync(projectToAdd);
                await AddAlertNotHandledNotificationRecipient(projectToAdd, dto.AlertNotHandledNotificationRecipientId, organizationId);
                await _nyssContext.SaveChangesAsync();

                return Success(projectToAdd.Id);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> Edit(int projectId, EditProjectRequestDto dto)
        {
            try
            {
                var projectToUpdate = await _nyssContext.Projects
                    .Include(p => p.ProjectOrganizations)
                    .Include(p => p.ProjectHealthRisks)
                    .ThenInclude(phr => phr.AlertRule)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (projectToUpdate == null)
                {
                    return Error(ResultKey.Project.ProjectDoesNotExist);
                }

                if (projectToUpdate.AllowMultipleOrganizations
                    && !await ValidateCoordinatorAccess(projectToUpdate.NationalSocietyId))
                {
                    return Error<int>(ResultKey.Project.OnlyCoordinatorCanAdministrateProjects);
                }

                if (!projectToUpdate.AllowMultipleOrganizations && dto.AllowMultipleOrganizations
                    && !await ValidateCoordinatorAccess(projectToUpdate.NationalSocietyId))
                {
                    return Error<int>(ResultKey.Project.OnlyCoordinatorCanAdministrateProjects);
                }

                if (dto.AllowMultipleOrganizations != projectToUpdate.AllowMultipleOrganizations && !_authorizationService.IsCurrentUserInAnyRole(Role.Coordinator, Role.Administrator))
                {
                    return Error<int>(ResultKey.Project.OnlyCoordinatorCanChangeMultipleOrgAccess);
                }

                if (projectToUpdate.AllowMultipleOrganizations && !dto.AllowMultipleOrganizations
                    && projectToUpdate.ProjectOrganizations.Count > 1)
                {
                    return Error<int>(ResultKey.Project.AllowMultipleOrganizationsFlagCannotBeRemoved);
                }

                projectToUpdate.Name = dto.Name;
                projectToUpdate.AllowMultipleOrganizations = dto.AllowMultipleOrganizations;

                await UpdateHealthRisks(projectToUpdate, dto.HealthRisks.ToList());

                await _nyssContext.SaveChangesAsync();

                return Success();
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result> Close(int projectId)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();

            var projectToClose = await _nyssContext.Projects
                .Select(p => new
                {
                    Project = p,
                    HasAccessAsHeadManager = p.ProjectOrganizations.Any(po => po.Organization.HeadManager.EmailAddress == currentUserName),
                    AnyOpenAlerts = p.ProjectHealthRisks.Any(phr => phr.Alerts.Any(a => a.Status == AlertStatus.Escalated || a.Status == AlertStatus.Open))
                })
                .SingleOrDefaultAsync(x => x.Project.Id == projectId);

            if (projectToClose == null)
            {
                return Error(ResultKey.Project.ProjectDoesNotExist);
            }

            if (projectToClose.Project.State == ProjectState.Closed)
            {
                return Error(ResultKey.Project.ProjectAlreadyClosed);
            }

            if (projectToClose.AnyOpenAlerts)
            {
                return Error(ResultKey.Project.ProjectHasOpenOrEscalatedAlerts);
            }

            if (projectToClose.Project.AllowMultipleOrganizations && !await ValidateCoordinatorAccess(projectToClose.Project.NationalSocietyId))
            {
                return Error<int>(ResultKey.Project.OnlyCoordinatorCanCloseThisProjects);
            }

            if (!projectToClose.Project.AllowMultipleOrganizations && !projectToClose.HasAccessAsHeadManager && !_authorizationService.IsCurrentUserInAnyRole(Role.Coordinator, Role.Administrator))
            {
                return Error(ResultKey.Unauthorized);
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                projectToClose.Project.State = ProjectState.Closed;
                projectToClose.Project.EndDate = _dateTimeProvider.UtcNow;
                var dataCollectorsToRemove = _nyssContext.DataCollectors.Where(dc => dc.Project.Id == projectId && !dc.RawReports.Any());

                await _dataCollectorService.AnonymizeDataCollectorsWithReports(projectId);
                _nyssContext.DataCollectors.RemoveRange(dataCollectorsToRemove);

                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();
            }

            return Success();
        }

        public async Task<Result<ProjectFormDataResponseDto>> GetFormData(int nationalSocietyId)
        {
            var contentLanguageId = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => ns.ContentLanguage.Id)
                .SingleAsync();

            var result = await GetFormDataDto(nationalSocietyId, contentLanguageId);
            return Success(result);
        }

        private IQueryable<Project> GetProjectsForSupervisorOrHeadSupervisor(User currentUser) =>
            currentUser.Role == Role.HeadSupervisor
                ? _nyssContext.HeadSupervisorUserProjects
                    .Where(hsup => hsup.HeadSupervisorUser == currentUser)
                    .Select(hsup => hsup.Project)
                : _nyssContext.SupervisorUserProjects
                    .Where(sup => sup.SupervisorUser == currentUser)
                    .Select(sup => sup.Project);

        private async Task<bool> ValidateCoordinatorAccess(int nationalSocietyId) =>
            _authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator)
            || !await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId && ns.NationalSocietyUsers.Any(nsu => nsu.User.Role == Role.Coordinator));

        private async Task UpdateHealthRisks(Project projectToUpdate, ICollection<ProjectHealthRiskRequestDto> healthRisks)
        {
            var projectHealthRiskIdsFromDto = healthRisks.Where(ar => ar.Id.HasValue).Select(ar => ar.Id.Value).ToList();

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

            var projectHealthRisksToAdd = healthRisks.Where(ar => ar.Id == null);
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

            var projectHealthRisksToUpdate = healthRisks.Where(ar => ar.Id.HasValue);
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

        private async Task<ProjectFormDataResponseDto> GetFormDataDto(int nationalSocietyId, int contentLanguageId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentUserOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User == currentUser && uns.NationalSocietyId == nationalSocietyId)
                .Select(uns => uns.OrganizationId)
                .FirstOrDefaultAsync();

            var data = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    HealthRisks = _nyssContext.HealthRisks
                        .Include(hr => hr.LanguageContents)
                        .Select(hr => new ProjectHealthRiskResponseDto
                        {
                            Id = null,
                            HealthRiskId = hr.Id,
                            HealthRiskType = hr.HealthRiskType,
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
                        .ToList(),
                    Organizations = ns.Organizations.Select(o => new ProjectFormDataResponseDto.ProjectFormOrganization
                    {
                        Id = o.Id,
                        Name = o.Name
                    }).ToList(),
                    AlertNotHandledRecipients = ns.NationalSocietyUsers
                        .Where(uns => (currentUser.Role == Role.Administrator || uns.OrganizationId == currentUserOrganizationId)
                            && (uns.User.Role == Role.Manager || uns.User.Role == Role.TechnicalAdvisor))
                        .Select(uns => new ProjectFormDataResponseDto.AlertNotHandledRecipientDto
                        {
                            Id = uns.UserId,
                            Name = uns.User.Name
                        })
                        .ToList()
                })
                .SingleAsync();

            return new ProjectFormDataResponseDto
            {
                Organizations = data.Organizations,
                HealthRisks = data.HealthRisks,
                AlertNotHandledRecipients = data.AlertNotHandledRecipients
            };
        }

        private async Task AddAlertNotHandledNotificationRecipient(Project project, int recipientUserId, int? organizationId)
        {
            var recipient = await _nyssContext.Users
                .Where(u => u.Id == recipientUserId)
                .Select(u => new
                {
                    User = u,
                    OrganizationId = u.UserNationalSocieties
                        .Where(uns => uns.NationalSocietyId == project.NationalSocietyId)
                        .Select(uns => uns.OrganizationId)
                        .First()
                })
                .FirstOrDefaultAsync();

            if (recipient == null)
            {
                throw new ResultException(ResultKey.Project.AlertNotHandledRecipientDoesNotExist);
            }

            if (!organizationId.HasValue || organizationId.Value != recipient.OrganizationId)
            {
                throw new ResultException(ResultKey.Project.AlertNotHandledNotificationRecipientMustBeOfSameOrg);
            }

            await _nyssContext.AlertNotHandledNotificationRecipients.AddAsync(new AlertNotHandledNotificationRecipient
            {
                User = recipient.User,
                Project = project,
                OrganizationId = organizationId.Value
            });
        }
    }
}
