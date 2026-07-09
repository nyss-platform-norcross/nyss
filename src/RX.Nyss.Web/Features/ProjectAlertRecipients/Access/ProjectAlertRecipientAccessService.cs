using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Access
{
    public interface IProjectAlertRecipientAccessService
    {
        Task<bool> HasCurrentUserAccessToAlertRecipients(int alertRecipientId);
    }

    public class ProjectAlertRecipientAccessService : IProjectAlertRecipientAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IProjectAccessService _projectAccessService;
        private readonly IAuthorizationService _authorizationService;

        public ProjectAlertRecipientAccessService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            IProjectAccessService projectAccessService)
        {
            _nyssContext = nyssContext;
            _projectAccessService = projectAccessService;
            _authorizationService = authorizationService;
        }

        public async Task<bool> HasCurrentUserAccessToAlertRecipients(int alertRecipientId)
        {
            var alertRecipient = await _nyssContext.AlertNotificationRecipients
                .Where(anr => anr.Id == alertRecipientId)
                .SingleAsync();

            var currentUser = await _authorizationService.GetCurrentUser();

            if (currentUser.Role == Role.Administrator)
            {
                return true;
            }

            var hasAccessToOrganization = _nyssContext.UserNationalSocieties
                .Any(un => un.UserId == currentUser.Id && un.OrganizationId == alertRecipient.OrganizationId);

            return hasAccessToOrganization && await _projectAccessService.HasCurrentUserAccessToProject(alertRecipient.ProjectId);
        }
    }
}
