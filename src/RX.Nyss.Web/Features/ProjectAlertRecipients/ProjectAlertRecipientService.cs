using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients
{
    public interface IProjectAlertRecipientService
    {
        Task<Result<List<ProjectAlertRecipientResponseDto>>> List(int nationalSocietyId, int projectId);
        Task<Result<ProjectAlertRecipientResponseDto>> Get(int alertRecipientId);
        Task<Result<int>> Create(int nationalSocietyId, int projectId, ProjectAlertRecipientRequestDto createDto);
        Task<Result> Edit(int alertRecipientId, ProjectAlertRecipientRequestDto editDto);
        Task<Result> Delete(int alertRecipientId);
        Task<Result<ProjectAlertRecipientFormDataDto>> GetFormData(int projectId);
    }

    public class ProjectAlertRecipientService : IProjectAlertRecipientService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public ProjectAlertRecipientService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<List<ProjectAlertRecipientResponseDto>>> List(int nationalSocietyId, int projectId)
        {
            var alertRecipientsQuery = _nyssContext.AlertNotificationRecipients
                .Where(anr => anr.ProjectId == projectId);

            if (!_authorizationService.IsCurrentUserInRole(Role.Administrator))
            {
                var currentUser = await _authorizationService.GetCurrentUser();
                var organizationId = _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                    .Select(uns => uns.OrganizationId)
                    .SingleOrDefault();

                alertRecipientsQuery = alertRecipientsQuery.Where(anr => anr.OrganizationId == organizationId);
            }

            var alertRecipients = await alertRecipientsQuery
                .OrderBy(anr => anr.Id)
                .Select(anr => new ProjectAlertRecipientResponseDto
                {
                    Id = anr.Id,
                    Role = anr.Role,
                    Organization = anr.Organization,
                    Email = anr.Email,
                    PhoneNumber = anr.PhoneNumber,
                    HealthRisks = anr.ProjectHealthRiskAlertRecipients.Select(phr => new ProjectAlertHealthRiskDto
                    {
                        Id = phr.ProjectHealthRiskId,
                        HealthRiskName = phr.ProjectHealthRisk.HealthRisk.LanguageContents.Single(lc => lc.ContentLanguageId == phr.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id).Name
                    }),
                    Supervisors = anr.SupervisorAlertRecipients.Select(sr => new ProjectAlertSupervisorsDto
                    {
                        Id = sr.SupervisorId,
                        Name = sr.Supervisor.Name,
                        OrganizationId = sr.Supervisor.UserNationalSocieties.Single(uns => uns.NationalSocietyId == nationalSocietyId).OrganizationId.Value
                    })
                })
                .ToListAsync();

            var result = Success(alertRecipients);

            return result;
        }

        public async Task<Result<ProjectAlertRecipientResponseDto>> Get(int alertRecipientId)
        {
            var alertRecipient = await _nyssContext.AlertNotificationRecipients
                .Where(anr => anr.Id == alertRecipientId)
                .Select(anr => new ProjectAlertRecipientResponseDto
                {
                    Id = anr.Id,
                    OrganizationId = anr.OrganizationId,
                    Role = anr.Role,
                    Organization = anr.Organization,
                    Email = anr.Email,
                    PhoneNumber = anr.PhoneNumber,
                    HealthRisks = anr.ProjectHealthRiskAlertRecipients.Select(phr => new ProjectAlertHealthRiskDto
                    {
                        Id = phr.ProjectHealthRiskId,
                        HealthRiskName = phr.ProjectHealthRisk.HealthRisk.LanguageContents.Single(lc => lc.ContentLanguageId == phr.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id).Name
                    }),
                    Supervisors = anr.SupervisorAlertRecipients.Select(sr => new ProjectAlertSupervisorsDto
                    {
                        Id = sr.SupervisorId,
                        Name = sr.Supervisor.Name,
                        OrganizationId = sr.Supervisor.UserNationalSocieties.Single().OrganizationId.Value
                    })
                }).SingleOrDefaultAsync();

            if (alertRecipient == null)
            {
                return Error<ProjectAlertRecipientResponseDto>(ResultKey.AlertRecipient.AlertRecipientDoesNotExist);
            }

            return Success(alertRecipient);
        }

        public async Task<Result<int>> Create(int nationalSocietyId, int projectId, ProjectAlertRecipientRequestDto createDto)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var projectIsClosed = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.EndDate.HasValue)
                .SingleOrDefaultAsync();

            if (projectIsClosed)
            {
                return Error<int>(ResultKey.AlertRecipient.ProjectIsClosed);
            }


            var organization = await _nyssContext.Organizations
                .Where(org => currentUser.Role == Role.Administrator
                    ? org.Id == createDto.OrganizationId && org.NationalSocietyId == nationalSocietyId
                    : org.NationalSocietyUsers.Any(nsu => nsu.UserId == currentUser.Id && org.NationalSocietyId == nationalSocietyId))
                .Select(org => new
                {
                    Id = org.Id,
                    OrganizationSupervisors = org.NationalSocietyUsers.Where(ns => ns.User.Role == Role.Supervisor).Select(u => u.UserId),
                    OrganizationHeadSupervisors = org.NationalSocietyUsers.Where(ns => ns.User.Role == Role.HeadSupervisor).Select(u => u.UserId)
                })
                .SingleOrDefaultAsync();

            if (organization == null)
            {
                return Error<int>(ResultKey.AlertRecipient.CurrentUserMustBeTiedToAnOrganization);
            }

            if (!createDto.Supervisors.All(s => organization.OrganizationSupervisors.Contains(s)))
            {
                return Error<int>(ResultKey.AlertRecipient.AllSupervisorsMustBeTiedToSameOrganization);
            }

            if (!createDto.HeadSupervisors.All(s => organization.OrganizationHeadSupervisors.Contains(s)))
            {
                return Error<int>(ResultKey.AlertRecipient.AllHeadSupervisorsMustBeTiedToSameOrganization);
            }

            var alertRecipientExistsForCurrentOrganization = await _nyssContext.AlertNotificationRecipients
                .AnyAsync(anr => anr.Email == createDto.Email && anr.PhoneNumber == createDto.PhoneNumber && anr.OrganizationId == organization.Id);

            if (alertRecipientExistsForCurrentOrganization)
            {
                return Error<int>(ResultKey.AlertRecipient.AlertRecipientAlreadyAdded);
            }

            var alertRecipientToAdd = new AlertNotificationRecipient
            {
                Role = createDto.Role,
                Organization = createDto.Organization,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                ProjectId = projectId,
                OrganizationId = organization.Id
            };

            await _nyssContext.AlertNotificationRecipients.AddAsync(alertRecipientToAdd);

            await _nyssContext.SupervisorUserAlertRecipients.AddRangeAsync(createDto.Supervisors.Select(supervisorId => new SupervisorUserAlertRecipient
            {
                SupervisorId = supervisorId,
                AlertNotificationRecipient = alertRecipientToAdd
            }));

            await _nyssContext.ProjectHealthRiskAlertRecipients.AddRangeAsync(createDto.HealthRisks.Select(healthRiskId => new ProjectHealthRiskAlertRecipient
            {
                ProjectHealthRiskId = healthRiskId,
                AlertNotificationRecipient = alertRecipientToAdd
            }));

            await _nyssContext.HeadSupervisorUserAlertRecipients.AddRangeAsync(createDto.HeadSupervisors.Select(headSupervisorId => new HeadSupervisorUserAlertRecipient
            {
                HeadSupervisorId = headSupervisorId,
                AlertNotificationRecipient = alertRecipientToAdd
            }));

            await _nyssContext.SaveChangesAsync();
            return Success(alertRecipientToAdd.Id);
        }

        public async Task<Result> Edit(int alertRecipientId, ProjectAlertRecipientRequestDto editDto)
        {
            var alertRecipient = await _nyssContext.AlertNotificationRecipients
                .Include(ar => ar.ProjectHealthRiskAlertRecipients)
                .Include(ar => ar.SupervisorAlertRecipients)
                .Where(anr => anr.Id == alertRecipientId)
                .SingleOrDefaultAsync();

            if (alertRecipient == null)
            {
                return Error(ResultKey.AlertRecipient.AlertRecipientDoesNotExist);
            }

            var projectIsClosed = await _nyssContext.Projects
                .Where(p => p.Id == alertRecipient.ProjectId)
                .Select(p => p.EndDate.HasValue)
                .SingleOrDefaultAsync();

            if (projectIsClosed)
            {
                return Error(ResultKey.AlertRecipient.ProjectIsClosed);
            }

            var applicableLinkedUsers = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.OrganizationId == alertRecipient.OrganizationId && (uns.User.Role == Role.Supervisor || uns.User.Role == Role.HeadSupervisor))
                .Select(u => new
                {
                    UserId = u.UserId,
                    Role = u.User.Role
                }).ToListAsync();

            if (!editDto.Supervisors.All(s => applicableLinkedUsers.Where(u => u.Role == Role.Supervisor).Select(u => u.UserId).Contains(s)))
            {
                return Error<int>(ResultKey.AlertRecipient.AllSupervisorsMustBeTiedToSameOrganization);
            }

            if (!editDto.HeadSupervisors.All(s => applicableLinkedUsers.Where(u => u.Role == Role.HeadSupervisor).Select(u => u.UserId).Contains(s)))
            {
                return Error<int>(ResultKey.AlertRecipient.AllHeadSupervisorsMustBeTiedToSameOrganization);
            }

            alertRecipient.Role = editDto.Role;
            alertRecipient.Organization = editDto.Organization;
            alertRecipient.Email = editDto.Email;
            alertRecipient.PhoneNumber = editDto.PhoneNumber;

            var supervisorsToAdd = editDto.Supervisors.Where(s => !alertRecipient.SupervisorAlertRecipients.Any(sar => sar.SupervisorId == s));
            var supervisorsToRemove = alertRecipient.SupervisorAlertRecipients.Where(sar => !editDto.Supervisors.Contains(sar.SupervisorId));

            var healthRisksToAdd = editDto.HealthRisks.Where(s => !alertRecipient.ProjectHealthRiskAlertRecipients.Any(har => har.ProjectHealthRiskId == s));
            var healthRisksToRemove = alertRecipient.ProjectHealthRiskAlertRecipients.Where(sar => !editDto.HealthRisks.Contains(sar.ProjectHealthRiskId));

            var headSupervisorsToAdd = editDto.HeadSupervisors.Where(s => !alertRecipient.HeadSupervisorUserAlertRecipients.Any(sar => sar.HeadSupervisorId == s));
            var headSupervisorsToRemove = alertRecipient.HeadSupervisorUserAlertRecipients.Where(sar => !editDto.HeadSupervisors.Contains(sar.HeadSupervisorId));

            await _nyssContext.SupervisorUserAlertRecipients.AddRangeAsync(supervisorsToAdd.Select(supervisorId => new SupervisorUserAlertRecipient
            {
                SupervisorId = supervisorId,
                AlertNotificationRecipientId = alertRecipientId
            }));
            _nyssContext.SupervisorUserAlertRecipients.RemoveRange(supervisorsToRemove);

            await _nyssContext.ProjectHealthRiskAlertRecipients.AddRangeAsync(healthRisksToAdd.Select(healthRiskId => new ProjectHealthRiskAlertRecipient
            {
                ProjectHealthRiskId = healthRiskId,
                AlertNotificationRecipientId = alertRecipientId
            }));
            _nyssContext.ProjectHealthRiskAlertRecipients.RemoveRange(healthRisksToRemove);

            await _nyssContext.HeadSupervisorUserAlertRecipients.AddRangeAsync(headSupervisorsToAdd.Select(headSupervisorId => new HeadSupervisorUserAlertRecipient
            {
                HeadSupervisorId = headSupervisorId,
                AlertNotificationRecipientId = alertRecipientId
            }));
            _nyssContext.HeadSupervisorUserAlertRecipients.RemoveRange(headSupervisorsToRemove);

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.AlertRecipient.AlertRecipientSuccessfullyEdited);
        }

        public async Task<Result> Delete(int alertRecipientId)
        {
            var alertRecipient = await _nyssContext.AlertNotificationRecipients
                .Include(anr => anr.SupervisorAlertRecipients)
                .Where(anr => anr.Id == alertRecipientId)
                .SingleOrDefaultAsync();

            if (alertRecipient == null)
            {
                return Error<int>(ResultKey.AlertRecipient.AlertRecipientDoesNotExist);
            }

            _nyssContext.AlertNotificationRecipients.Remove(alertRecipient);

            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<ProjectAlertRecipientFormDataDto>> GetFormData(int projectId)
        {
            var isAdmin = _authorizationService.IsCurrentUserInRole(Role.Administrator);
            var nationalSocietyId = await _nyssContext.Projects.Where(p => p.Id == projectId).Select(p => p.NationalSocietyId).SingleOrDefaultAsync();

            var currentUserOrganizationId = isAdmin
                ? 0
                : await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.NationalSocietyId == nationalSocietyId && uns.User.EmailAddress == _authorizationService.GetCurrentUserName())
                    .Select(uns => uns.Organization.Id)
                    .SingleOrDefaultAsync();

            var organizations = await _nyssContext.Organizations.Where(o => isAdmin
                    ? o.NationalSocietyId == nationalSocietyId
                    : o.NationalSocietyId == nationalSocietyId && o.Id == currentUserOrganizationId)
                .Select(o => new ProjectAlertOrganization
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToListAsync();

            var supervisors = await _nyssContext.SupervisorUserProjects.FilterAvailableUsers().Where(sup => isAdmin
                    ? sup.SupervisorUser.CurrentProject.Id == projectId
                    : sup.SupervisorUser.CurrentProject.Id == projectId && sup.SupervisorUser.UserNationalSocieties.Any(uns => uns.OrganizationId == currentUserOrganizationId))
                .Select(s => new ProjectAlertSupervisorsDto
                {
                    Id = s.SupervisorUser.Id,
                    Name = s.SupervisorUser.Name,
                    OrganizationId = s.SupervisorUser.UserNationalSocieties.Single(uns => uns.NationalSocietyId == nationalSocietyId).OrganizationId.Value
                }).ToListAsync();

            var healthRisks = await _nyssContext.ProjectHealthRisks
                .Where(phr => phr.Project.Id == projectId && phr.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                .Select(phr => new ProjectAlertHealthRiskDto
                {
                    Id = phr.Id,
                    HealthRiskName = phr.HealthRisk.LanguageContents.Single(lc => lc.ContentLanguageId == phr.Project.NationalSociety.ContentLanguage.Id).Name
                }).ToListAsync();

            return Success(new ProjectAlertRecipientFormDataDto
            {
                Supervisors = supervisors,
                ProjectOrganizations = organizations,
                HealthRisks = healthRisks
            });
        }
    }
}
