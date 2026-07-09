using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Access
{
    public interface IProjectAlertNotHandledRecipientAccessService
    {
        Task<bool> HasAccessToCreateForOrganization(int organizationId);
        Task<bool> UserIsInOrganization(int userId, int organizationId);
    }

    public class ProjectAlertNotHandledRecipientAccessService : IProjectAlertNotHandledRecipientAccessService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INyssContext _nyssContext;

        public ProjectAlertNotHandledRecipientAccessService(IAuthorizationService authorizationService, INyssContext nyssContext)
        {
            _authorizationService = authorizationService;
            _nyssContext = nyssContext;
        }

        public async Task<bool> HasAccessToCreateForOrganization(int organizationId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            return currentUser.Role == Role.Administrator || await _nyssContext.UserNationalSocieties
                .AnyAsync(uns => uns.User == currentUser && uns.OrganizationId == organizationId);
        }

        public async Task<bool> UserIsInOrganization(int userId, int organizationId) =>
            await _nyssContext.UserNationalSocieties
                .AnyAsync(uns => uns.UserId == userId && uns.OrganizationId == organizationId);
    }
}
