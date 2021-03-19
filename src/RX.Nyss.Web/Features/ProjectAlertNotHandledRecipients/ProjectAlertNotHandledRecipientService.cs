using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients
{
    public interface IProjectAlertNotHandledRecipientService
    {
        Task<Result> Create(int projectId, int recipientUserId);
        Task<Result> Edit(int projectId, ProjectAlertNotHandledRecipientRequestDto dto);
        Task<Result<List<ProjectAlertNotHandledRecipientResponseDto>>> List(int projectId);
        Task<Result<List<ProjectAlertNotHandledRecipientResponseDto>>> GetFormData(int projectId);
    }

    public class ProjectAlertNotHandledRecipientService : IProjectAlertNotHandledRecipientService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILoggerAdapter _loggerAdapter;

        public ProjectAlertNotHandledRecipientService(INyssContext nyssContext, IAuthorizationService authorizationService, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result> Create(int projectId, int recipientUserId)
        {
            var exists = await _nyssContext.AlertNotHandledNotificationRecipients.AnyAsync(a => a.ProjectId == projectId && a.UserId == recipientUserId);
            if (exists)
            {
                return Error(ResultKey.AlertNotHandledNotificationRecipient.AlreadyExists);
            }

            var alertNotHandledNotificationRecipient = new AlertNotHandledNotificationRecipient
            {
                ProjectId = projectId,
                UserId = recipientUserId
            };

            await _nyssContext.AlertNotHandledNotificationRecipients.AddAsync(alertNotHandledNotificationRecipient);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.AlertNotHandledNotificationRecipient.CreateSuccess);
        }

        public async Task<Result> Edit(int projectId, ProjectAlertNotHandledRecipientRequestDto dto)
        {
            var currentAlertNotHandledRecipient = await _nyssContext.AlertNotHandledNotificationRecipients
                .Where(a => a.ProjectId == projectId
                    && _nyssContext.UserNationalSocieties.Any(uns => uns.UserId == a.UserId && uns.OrganizationId == dto.OrganizationId))
                .SingleOrDefaultAsync();

            if (currentAlertNotHandledRecipient == null)
            {
                return Error(ResultKey.AlertNotHandledNotificationRecipient.NotFound);
            }

            var alertNotHandledNotificationRecipient = new AlertNotHandledNotificationRecipient
            {
                ProjectId = projectId,
                UserId = dto.UserId
            };

            _nyssContext.AlertNotHandledNotificationRecipients.Remove(currentAlertNotHandledRecipient);
            await _nyssContext.AlertNotHandledNotificationRecipients.AddAsync(alertNotHandledNotificationRecipient);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.AlertNotHandledNotificationRecipient.EditSuccess);
        }

        public async Task<Result<List<ProjectAlertNotHandledRecipientResponseDto>>> List(int projectId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentUserOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User == currentUser)
                .Select(uns => uns.OrganizationId)
                .FirstOrDefaultAsync();

            var alertNotHandledRecipientsForOrganization = await _nyssContext.ProjectOrganizations
                .Where(po => po.ProjectId == projectId && (currentUser.Role == Role.Administrator || po.OrganizationId == currentUserOrganizationId))
                .Select(po => new
                {
                    Organization = po.Organization,
                    AlertNotHandledRecipient = po.Project.AlertNotHandledNotificationRecipients
                        .Where(ar => _nyssContext.UserNationalSocieties
                            .Any(uns => uns.UserId == ar.UserId && uns.OrganizationId == po.OrganizationId))
                        .Select(ar => ar.User)
                        .FirstOrDefault()
                }).ToListAsync();

            var alertNotHandledRecipients = alertNotHandledRecipientsForOrganization.Select(x => new ProjectAlertNotHandledRecipientResponseDto
            {
                OrganizationId = x.Organization.Id,
                OrganizationName = x.Organization.Name,
                UserId = x.AlertNotHandledRecipient?.Id,
                Name = x.AlertNotHandledRecipient?.Name
            }).ToList();

            return Success(alertNotHandledRecipients);
        }

        public async Task<Result<List<ProjectAlertNotHandledRecipientResponseDto>>> GetFormData(int projectId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentUserOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User == currentUser)
                .Select(uns => uns.OrganizationId)
                .FirstOrDefaultAsync();

            var recipients = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers
                    .Where(nsu => (nsu.User.Role == Role.Manager || nsu.User.Role == Role.TechnicalAdvisor)
                        && (currentUser.Role == Role.Administrator || currentUserOrganizationId == nsu.OrganizationId)))
                .Select(nsu => new ProjectAlertNotHandledRecipientResponseDto
                {
                    OrganizationId = nsu.OrganizationId.Value,
                    OrganizationName = nsu.Organization.Name,
                    Name = nsu.User.Name,
                    UserId = nsu.UserId
                })
                .ToListAsync();

            return Success(recipients);
        }
    }
}
