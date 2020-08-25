using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients
{
    public interface IProjectAlertRecipientService
    {
        Task<Result<List<ProjectAlertRecipientListResponseDto>>> List(int nationalSocietyId, int projectId);
        Task<Result<ProjectAlertRecipientListResponseDto>> Get(int alertRecipientId);
        Task<Result<int>> Create(int nationalSocietyId, int projectId, ProjectAlertRecipientRequestDto createDto);
        Task<Result> Edit(int alertRecipientId, ProjectAlertRecipientRequestDto editDto);
        Task<Result> Delete(int alertRecipientId);
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

        public async Task<Result<List<ProjectAlertRecipientListResponseDto>>> List(int nationalSocietyId, int projectId)
        {
            var alertRecipientsQuery = _nyssContext.AlertNotificationRecipients
                .Where(anr => anr.ProjectId == projectId);

            if (!_authorizationService.IsCurrentUserInRole(Role.Administrator))
            {
                var currentUser = await _authorizationService.GetCurrentUserAsync();
                var organizationId = _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                    .Select(uns => uns.OrganizationId)
                    .SingleOrDefault();

                alertRecipientsQuery = alertRecipientsQuery.Where(anr => anr.OrganizationId == organizationId);
            }

            var alertRecipients = await alertRecipientsQuery
                .OrderBy(anr => anr.Id)
                .Select(anr => new ProjectAlertRecipientListResponseDto
                {
                    Id = anr.Id,
                    Role = anr.Role,
                    Organization = anr.Organization,
                    Email = anr.Email,
                    PhoneNumber = anr.PhoneNumber,
                    HealthRisks = anr.ProjectHealthRiskAlertRecipients.Select(phr => phr.ProjectHealthRisk.HealthRisk.HealthRiskCode),
                    Supervisors = anr.SupervisorAlertRecipients.Select(sr => sr.Supervisor.Name)
                })
                .ToListAsync();

            var result = Success(alertRecipients);

            return result;
        }

        public async Task<Result<ProjectAlertRecipientListResponseDto>> Get(int alertRecipientId)
        {
            var alertRecipient = await _nyssContext.AlertNotificationRecipients
                .Where(anr => anr.Id == alertRecipientId)
                .Select(anr => new ProjectAlertRecipientListResponseDto
                {
                    Id = anr.Id,
                    Role = anr.Role,
                    Organization = anr.Organization,
                    Email = anr.Email,
                    PhoneNumber = anr.PhoneNumber
                })
                .SingleOrDefaultAsync();

            if (alertRecipient == null)
            {
                return Error<ProjectAlertRecipientListResponseDto>(ResultKey.AlertRecipient.AlertRecipientDoesNotExist);
            }

            return Success(alertRecipient);
        }

        public async Task<Result<int>> Create(int nationalSocietyId, int projectId, ProjectAlertRecipientRequestDto createDto)
        {
            var currentUser = await _authorizationService.GetCurrentUserAsync();

            var projectIsClosed = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.EndDate.HasValue)
                .SingleOrDefaultAsync();

            if (projectIsClosed)
            {
                return Error<int>(ResultKey.AlertRecipient.ProjectIsClosed);
            }

            var organizationId = createDto.OrganizationId ?? await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                    .Select(uns => uns.OrganizationId)
                    .SingleOrDefaultAsync();

            if (!organizationId.HasValue)
            {
                return Error<int>(ResultKey.AlertRecipient.CurrentUserMustBeTiedToAnOrganization);
            }

            var alertRecipientExistsForCurrentOrganization = await _nyssContext.AlertNotificationRecipients
                .AnyAsync(anr => anr.Email == createDto.Email && anr.PhoneNumber == createDto.PhoneNumber && anr.OrganizationId == organizationId);

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
                OrganizationId = organizationId.Value
            };

            await _nyssContext.AlertNotificationRecipients.AddAsync(alertRecipientToAdd);
            await _nyssContext.SaveChangesAsync();

            return Success(alertRecipientToAdd.Id);
        }

        public async Task<Result> Edit(int alertRecipientId, ProjectAlertRecipientRequestDto editDto)
        {
            var alertRecipient = await _nyssContext.AlertNotificationRecipients
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

            alertRecipient.Role = editDto.Role;
            alertRecipient.Organization = editDto.Organization;
            alertRecipient.Email = editDto.Email;
            alertRecipient.PhoneNumber = editDto.PhoneNumber;

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

            if (alertRecipient.SupervisorAlertRecipients.Any())
            {
                return Error<int>(ResultKey.AlertRecipient.CannotDeleteAlertRecipientTiedToSupervisors);
            }

            _nyssContext.AlertNotificationRecipients.Remove(alertRecipient);

            await _nyssContext.SaveChangesAsync();
            return Success();
        }
    }
}
