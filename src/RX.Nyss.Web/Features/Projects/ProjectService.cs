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
using System;

namespace RX.Nyss.Web.Features.Projects
{
    public interface IProjectService
    {
        Task<Result<ProjectResponseDto>> Get(int projectId);
        Task<Result<List<ProjectListItemResponseDto>>> List(int nationalSocietyId, int utcOffset);
        Task<Result<int>> Create(int nationalSocietyId, CreateProjectRequestDto dto);
        Task<Result> Edit(int projectId, EditProjectRequestDto dto);
        Task<Result> EditProjectHealthRisks(int projectId, EditHealthRisksRequestDto dto);
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
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService,
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
                    ProjectHealthRisks = p.ProjectHealthRisks
                        .OrderBy(phr => phr.HealthRisk.HealthRiskCode)
                        .Select(phr => new ProjectHealthRiskResponseDto
                        {
                            Id = phr.Id,
                            HealthRiskId = phr.HealthRiskId,
                            HealthRiskType = phr.HealthRisk.HealthRiskType,
                            HealthRiskCode = phr.HealthRisk.HealthRiskCode,
                            // Safe: guard both navigation properties before accessing Id
                            HealthRiskName = phr.HealthRisk.LanguageContents
                                .Where(lc =>
                                    lc.ContentLanguage != null &&
                                    p.NationalSociety != null &&
                                    p.NationalSociety.ContentLanguage != null &&
                                    lc.ContentLanguage.Id == p.NationalSociety.ContentLanguage.Id)
                                .Select(lc => lc.Name)
                                .FirstOrDefault() ?? string.Empty,
                            // Safe: AlertRule may be null if no rule was configured
                            AlertRuleCountThreshold = phr.AlertRule != null ? phr.AlertRule.CountThreshold : (int?)null,
                            AlertRuleDaysThreshold = phr.AlertRule != null ? phr.AlertRule.DaysThreshold : null,
                            AlertRuleKilometersThreshold = phr.AlertRule != null ? phr.AlertRule.KilometersThreshold : null,
                            FeedbackMessage = phr.FeedbackMessage ?? string.Empty,
                            CaseDefinition = phr.CaseDefinition ?? string.Empty,
                            ContainsReports = phr.Reports.Any()
                        }),
                    // Safe: default to 1 when NationalSociety or ContentLanguage is null
                    ContentLanguageId = p.NationalSociety != null && p.NationalSociety.ContentLanguage != null
                        ? p.NationalSociety.ContentLanguage.Id
                        : 1,
                    HasCoordinator = p.NationalSociety != null &&
                                     p.NationalSociety.NationalSocietyUsers!
                                         .Any(nsu => nsu.User != null && nsu.User.Role == Role.Coordinator)
                })
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Error<ProjectResponseDto>(ResultKey.Project.ProjectDoesNotExist);
            }

            project.FormData = await GetFormDataDto(project.NationalSocietyId, project.ContentLanguageId);

            return Success(project);
        }

        public async Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(
            int projectId,
            IEnumerable<HealthRiskType> healthRiskTypes)
        {
            // Guard: avoid query with null/empty collection
            var typeList = healthRiskTypes?.ToList() ?? new List<HealthRiskType>();

            return await _nyssContext.ProjectHealthRisks
                .Where(ph => ph.Project.Id == projectId && typeList.Contains(ph.HealthRisk.HealthRiskType))
                .Select(ph => new HealthRiskDto
                {
                    Id = ph.HealthRiskId,
                    // Safe: language filter was commented out; take first available name
                    Name = ph.HealthRisk.LanguageContents
                        .Select(lc => lc.Name)
                        .FirstOrDefault() ?? string.Empty
                })
                .OrderBy(x => x.Name)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Result<List<ProjectListItemResponseDto>>> List(int nationalSocietyId, int utcOffset)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            // Guard: currentUser may be null in edge cases
            if (currentUser == null)
            {
                return Error<List<ProjectListItemResponseDto>>(ResultKey.Unauthorized);
            }

            var projectsQuery = _authorizationService.IsCurrentUserInAnyRole(Role.HeadSupervisor, Role.Supervisor)
                ? GetProjectsForSupervisorOrHeadSupervisor(currentUser)
                : _nyssContext.Projects;

            var nationalSocietyData = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    CurrentUserOrganizationId = ns.NationalSocietyUsers!
                        .Where(uns => uns.User == currentUser)
                        .Select(uns => uns.OrganizationId)
                        .SingleOrDefault(),
                    HasCoordinator = ns.NationalSocietyUsers!
                        .Any(uns => uns.User != null && uns.User.Role == Role.Coordinator)
                })
                .SingleOrDefaultAsync();

            // Guard: national society not found
            if (nationalSocietyData == null)
            {
                return Error<List<ProjectListItemResponseDto>>(ResultKey.Project.NationalSocietyDoesNotExist);
            }

            var query = projectsQuery
                .Where(p => p.NationalSocietyId == nationalSocietyId);

            if (nationalSocietyData.HasCoordinator &&
                !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator, Role.DataConsumer))
            {
                query = query.Where(p =>
                    p.ProjectOrganizations.Any(po => po.OrganizationId == nationalSocietyData.CurrentUserOrganizationId));
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
                    EndDate = p.EndDate.HasValue ? p.EndDate.Value.AddHours(utcOffset) : (DateTime?)null,
                    IsClosed = p.State == ProjectState.Closed,
                    TotalReportCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Reports)
                        .Where(r =>
                            r.ProjectHealthRisk != null &&
                            r.ProjectHealthRisk.HealthRisk != null &&
                            r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity &&
                            !r.IsTraining)
                        .Sum(r => r.ReportedCaseCount),
                    EscalatedAlertCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Alerts)
                        .Count(a => a.EscalatedAt.HasValue),
                    TotalDataCollectorCount = p.DataCollectors
                        .Count(dc => dc.Name != Anonymization.Text && dc.DeletedAt == null),
                    SupervisorCount = p.SupervisorUserProjects.Count + p.HeadSupervisorUserProjects.Count
                })
                .AsSplitQuery()
                .ToListAsync();

            return Success(projects);
        }

        public async Task<Result<int>> Create(int nationalSocietyId, CreateProjectRequestDto dto)
        {
            // Guard: null DTO
            if (dto == null)
            {
                return Error<int>(ResultKey.Project.ProjectDoesNotExist);
            }

            try
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                // Guard: currentUser may be null
                if (currentUser == null)
                {
                    return Error<int>(ResultKey.Unauthorized);
                }

                var data = await _nyssContext.NationalSocieties
                    .Where(ns => ns.Id == nationalSocietyId)
                    .Select(ns => new
                    {
                        ns.IsArchived,
                        OrganizationsCount = ns.Organizations != null ? ns.Organizations.Count : 1,
                        FirstOrganizationId = ns.Organizations != null
                            ? ns.Organizations.Select(o => (int?)o.Id).FirstOrDefault()
                            : null,
                        UserOrganizationId = ns.NationalSocietyUsers != null
                            ? ns.NationalSocietyUsers
                                .Where(nsu => nsu.User == currentUser && nsu.NationalSocietyId == nationalSocietyId)
                                .Select(nsu => (int?)nsu.OrganizationId)
                                .FirstOrDefault()
                            : null
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

                // Guard: HealthRisks list may be null
                var healthRisksFromDto = dto.HealthRisks?.ToList() ?? new List<ProjectHealthRiskRequestDto>();

                var healthRiskIdsInDatabase = await _nyssContext.HealthRisks
                    .Select(hr => hr.Id)
                    .ToListAsync();

                var healthRiskIdsToAttach = healthRisksFromDto
                    .Select(hr => hr.HealthRiskId)
                    .ToList();

                if (!healthRiskIdsToAttach.All(id => healthRiskIdsInDatabase.Contains(id)))
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

                    ProjectHealthRisks = healthRisksFromDto.Select(phr => new ProjectHealthRisk
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

                // Guard: only add notification recipient when an ID is provided
                if (dto.AlertNotHandledNotificationRecipientId > 0)
                {
                    await AddAlertNotHandledNotificationRecipient(
                        projectToAdd,
                        dto.AlertNotHandledNotificationRecipientId,
                        organizationId);
                }

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
            // Guard: null DTO
            if (dto == null)
            {
                return Error(ResultKey.Project.ProjectDoesNotExist);
            }

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

                if (projectToUpdate.AllowMultipleOrganizations &&
                    !await ValidateCoordinatorAccess(projectToUpdate.NationalSocietyId))
                {
                    return Error(ResultKey.Project.OnlyCoordinatorCanAdministrateProjects);
                }

                if (!projectToUpdate.AllowMultipleOrganizations &&
                    dto.AllowMultipleOrganizations &&
                    !await ValidateCoordinatorAccess(projectToUpdate.NationalSocietyId))
                {
                    return Error(ResultKey.Project.OnlyCoordinatorCanAdministrateProjects);
                }

                if (dto.AllowMultipleOrganizations != projectToUpdate.AllowMultipleOrganizations &&
                    !_authorizationService.IsCurrentUserInAnyRole(Role.Coordinator, Role.Administrator))
                {
                    return Error(ResultKey.Project.OnlyCoordinatorCanChangeMultipleOrgAccess);
                }

                if (projectToUpdate.AllowMultipleOrganizations &&
                    !dto.AllowMultipleOrganizations &&
                    (projectToUpdate.ProjectOrganizations?.Count ?? 0) > 1)
                {
                    return Error(ResultKey.Project.AllowMultipleOrganizationsFlagCannotBeRemoved);
                }

                projectToUpdate.Name = dto.Name;
                projectToUpdate.AllowMultipleOrganizations = dto.AllowMultipleOrganizations;

                await _nyssContext.SaveChangesAsync();

                return Success();
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result ?? Error(ResultKey.Project.ProjectUpdateError);
            }
        }

        public async Task<Result> EditProjectHealthRisks(int projectId, EditHealthRisksRequestDto dto)
        {
            // Guard: null DTO
            if (dto == null)
            {
                return Error(ResultKey.Project.ProjectDoesNotExist);
            }

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

                if (projectToUpdate.AllowMultipleOrganizations &&
                    !await ValidateCoordinatorAccess(projectToUpdate.NationalSocietyId))
                {
                    return Error(ResultKey.Project.OnlyCoordinatorCanAdministrateProjects);
                }

                // Guard: HealthRisks collection may be null
                var healthRisks = dto.HealthRisks?.ToList() ?? new List<ProjectHealthRiskRequestDto>();
                await UpdateHealthRisks(projectToUpdate, healthRisks);

                await _nyssContext.SaveChangesAsync();

                return Success();
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result ?? Error(ResultKey.Project.ProjectUpdateError);
            }
        }

        public async Task<Result> Close(int projectId)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();

            var projectToClose = await _nyssContext.Projects
                .Select(p => new
                {
                    Project = p,
                    // Safe: guard HeadManager navigation before accessing EmailAddress
                    HasAccessAsHeadManager = p.ProjectOrganizations.Any(po =>
                        po.Organization != null &&
                        po.Organization.HeadManager != null &&
                        po.Organization.HeadManager.EmailAddress == currentUserName),
                    AnyOpenAlerts = p.ProjectHealthRisks.Any(phr =>
                        phr.Alerts.Any(a =>
                            a.Status == AlertStatus.Escalated ||
                            a.Status == AlertStatus.Open))
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

            if (projectToClose.Project.AllowMultipleOrganizations &&
                !await ValidateCoordinatorAccess(projectToClose.Project.NationalSocietyId))
            {
                return Error(ResultKey.Project.OnlyCoordinatorCanCloseThisProjects);
            }

            if (!projectToClose.Project.AllowMultipleOrganizations &&
                !projectToClose.HasAccessAsHeadManager &&
                !_authorizationService.IsCurrentUserInAnyRole(Role.Coordinator, Role.Administrator))
            {
                return Error(ResultKey.Unauthorized);
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                projectToClose.Project.State = ProjectState.Closed;
                projectToClose.Project.EndDate = _dateTimeProvider.UtcNow;

                var dataCollectorsToRemove = _nyssContext.DataCollectors
                    .Where(dc => dc.Project != null && dc.Project.Id == projectId && (dc.RawReports == null || !dc.RawReports.Any()));

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
                .Select(ns => ns.ContentLanguage != null ? ns.ContentLanguage.Id : 1) // Safe: default to 1
                .SingleOrDefaultAsync();

            // Guard: society not found returns 0 from SingleOrDefaultAsync
            if (contentLanguageId == 0)
            {
                return Error<ProjectFormDataResponseDto>(ResultKey.Project.NationalSocietyDoesNotExist);
            }

            var result = await GetFormDataDto(nationalSocietyId, contentLanguageId);
            return Success(result);
        }

        // -------------------------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------------------------

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
            || !await _nyssContext.NationalSocieties
                .AnyAsync(ns =>
                    ns.Id == nationalSocietyId &&
                    ns.NationalSocietyUsers!.Any(nsu => nsu.User != null && nsu.User.Role == Role.Coordinator));

        private async Task UpdateHealthRisks(Project projectToUpdate, ICollection<ProjectHealthRiskRequestDto> healthRisks)
        {
            var projectHealthRiskIdsFromDto = healthRisks
                .Where(ar => ar.Id.HasValue)
                .Select(ar => ar.Id!.Value)
                .ToList();

            var projectHealthRisksToDelete = await _nyssContext.ProjectHealthRisks
                .Where(phr =>
                    phr.Project.Id == projectToUpdate.Id &&
                    !projectHealthRiskIdsFromDto.Contains(phr.Id))
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

            _nyssContext.ProjectHealthRisks.RemoveRange(
                projectHealthRisksToDelete.Select(phr => phr.ProjectHealthRisk));

            // Add new health risks (no Id yet)
            foreach (var phr in healthRisks.Where(ar => ar.Id == null))
            {
                projectToUpdate.ProjectHealthRisks.Add(new ProjectHealthRisk
                {
                    HealthRiskId = phr.HealthRiskId,
                    CaseDefinition = phr.CaseDefinition,
                    FeedbackMessage = phr.FeedbackMessage,
                    AlertRule = new AlertRule
                    {
                        CountThreshold = phr.AlertRuleCountThreshold ?? 0,
                        DaysThreshold = phr.AlertRuleDaysThreshold,
                        KilometersThreshold = phr.AlertRuleKilometersThreshold
                    }
                });
            }

            // Update existing health risks
            foreach (var phr in healthRisks.Where(ar => ar.Id.HasValue))
            {
                var existing = projectToUpdate.ProjectHealthRisks
                    .FirstOrDefault(ar => ar.Id == phr.Id!.Value);

                if (existing == null)
                {
                    continue; // Safe: skip if not found rather than throw NRE
                }

                existing.CaseDefinition = phr.CaseDefinition;
                existing.FeedbackMessage = phr.FeedbackMessage;

                // Guard: AlertRule may be null for legacy records
                if (existing.AlertRule == null)
                {
                    existing.AlertRule = new AlertRule();
                }

                existing.AlertRule.CountThreshold = phr.AlertRuleCountThreshold ?? 0;
                existing.AlertRule.DaysThreshold = phr.AlertRuleDaysThreshold;
                existing.AlertRule.KilometersThreshold = phr.AlertRuleKilometersThreshold;
            }
        }

        private async Task<ProjectFormDataResponseDto> GetFormDataDto(int nationalSocietyId, int contentLanguageId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            // Guard: null user — return empty form data rather than throw
            if (currentUser == null)
            {
                return new ProjectFormDataResponseDto
                {
                    Organizations = new List<ProjectFormDataResponseDto.ProjectFormOrganization>(),
                    HealthRisks = new List<ProjectHealthRiskResponseDto>(),
                    AlertNotHandledRecipients = new List<ProjectFormDataResponseDto.AlertNotHandledRecipientDto>()
                };
            }

            var currentUserOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User == currentUser && uns.NationalSocietyId == nationalSocietyId)
                .Select(uns => uns.OrganizationId)
                .FirstOrDefaultAsync();

            var data = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    HealthRisks = _nyssContext.HealthRisks
                        .Select(hr => new ProjectHealthRiskResponseDto
                        {
                            Id = null,
                            HealthRiskId = hr.Id,
                            HealthRiskType = hr.HealthRiskType,
                            HealthRiskCode = hr.HealthRiskCode,
                            // Safe: filter by ContentLanguage id only when it exists
                            HealthRiskName = hr.LanguageContents
                                .Where(lc => lc.ContentLanguage != null && lc.ContentLanguage.Id == contentLanguageId)
                                .Select(lc => lc.Name)
                                .FirstOrDefault() ?? string.Empty,
                            AlertRuleCountThreshold = _nyssContext.AlertRules
                                .Where(ar => ar.Id == EF.Property<int?>(hr, "AlertRuleId"))
                                .Select(ar => (int?)ar.CountThreshold)
                                .FirstOrDefault(),
                            AlertRuleDaysThreshold = _nyssContext.AlertRules
                                .Where(ar => ar.Id == EF.Property<int?>(hr, "AlertRuleId"))
                                .Select(ar => ar.DaysThreshold)
                                .FirstOrDefault(),
                            AlertRuleKilometersThreshold = _nyssContext.AlertRules
                                .Where(ar => ar.Id == EF.Property<int?>(hr, "AlertRuleId"))
                                .Select(ar => ar.KilometersThreshold)
                                .FirstOrDefault(),
                            FeedbackMessage = hr.LanguageContents
                                .Where(lc => lc.ContentLanguage != null && lc.ContentLanguage.Id == contentLanguageId)
                                .Select(lc => lc.FeedbackMessage)
                                .FirstOrDefault() ?? string.Empty,
                            CaseDefinition = hr.LanguageContents
                                .Where(lc => lc.ContentLanguage != null && lc.ContentLanguage.Id == contentLanguageId)
                                .Select(lc => lc.CaseDefinition)
                                .FirstOrDefault() ?? string.Empty,
                            ContainsReports = false
                        })
                        .OrderBy(hr => hr.HealthRiskCode)
                        .ToList(),
                    Organizations = ns.Organizations!
                        .Select(o => new ProjectFormDataResponseDto.ProjectFormOrganization
                        {
                            Id = o.Id,
                            Name = o.Name
                        })
                        .ToList(),
                    // Safe: guard User navigation before accessing Role/Name
                    AlertNotHandledRecipients = ns.NationalSocietyUsers!
                        .Where(uns =>
                            uns.User != null &&
                            (currentUser.Role == Role.Administrator || uns.OrganizationId == currentUserOrganizationId) &&
                            (uns.User.Role == Role.Manager || uns.User.Role == Role.TechnicalAdvisor))
                        .Select(uns => new ProjectFormDataResponseDto.AlertNotHandledRecipientDto
                        {
                            Id = uns.UserId,
                            Name = uns.User!.Name ?? string.Empty,
                            OrganizationId = uns.OrganizationId
                        })
                        .ToList()
                })
                .SingleOrDefaultAsync();

            // Guard: national society not found
            if (data == null)
            {
                return new ProjectFormDataResponseDto
                {
                    Organizations = new List<ProjectFormDataResponseDto.ProjectFormOrganization>(),
                    HealthRisks = new List<ProjectHealthRiskResponseDto>(),
                    AlertNotHandledRecipients = new List<ProjectFormDataResponseDto.AlertNotHandledRecipientDto>()
                };
            }

            return new ProjectFormDataResponseDto
            {
                Organizations = data.Organizations,
                HealthRisks = data.HealthRisks,
                AlertNotHandledRecipients = data.AlertNotHandledRecipients
            };
        }

        private async Task AddAlertNotHandledNotificationRecipient(
            Project project,
            int recipientUserId,
            int? organizationId)
        {
            var recipient = await _nyssContext.Users
                .Where(u => u.Id == recipientUserId)
                .Select(u => new
                {
                    User = u,
                    OrganizationId = u.UserNationalSocieties!
                        .Where(uns => uns.NationalSocietyId == project.NationalSocietyId)
                        .Select(uns => uns.OrganizationId)
                        .FirstOrDefault()
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