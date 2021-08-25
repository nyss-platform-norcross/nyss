using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Projects.Access
{
    public interface IProjectAccessService
    {
        Task<bool> HasCurrentUserAccessToProject(int projectId);
        bool HasCurrentUserAccessToAssignOrganizationToProject();
        Task<bool> HasAccessToAlertNotHandledNotificationRecipient(int userId);
    }

    public class ProjectAccessService : IProjectAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public ProjectAccessService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            INationalSocietyAccessService nationalSocietyAccessService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToProject(int projectId)
        {
            if (_authorizationService.IsCurrentUserInRole(Role.Supervisor))
            {
                return await HasSupervisorAccessToProject(_authorizationService.GetCurrentUserName(), projectId);
            }

            if (_authorizationService.IsCurrentUserInRole(Role.HeadSupervisor))
            {
                return await HasHeadSupervisorAccessToProject(_authorizationService.GetCurrentUserName(), projectId);
            }

            var currentUser = await _authorizationService.GetCurrentUser();

            var data = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new
                {
                    p.NationalSocietyId,
                    HasCoordinator = p.NationalSociety.NationalSocietyUsers.Any(uns => uns.User.Role == Role.Coordinator),
                    HasSameOrganization = p.ProjectOrganizations.Any(po =>
                        po.OrganizationId == p.NationalSociety.NationalSocietyUsers
                                                .Where(uns => uns.User == currentUser)
                                                .Select(uns => uns.OrganizationId)
                                                .FirstOrDefault()
                    )
                })
                .SingleAsync();

            if (!_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator, Role.DataConsumer) && data.HasCoordinator && !data.HasSameOrganization)
            {
                return false;
            }

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(data.NationalSocietyId);
        }

        public async Task<bool> HasAccessToAlertNotHandledNotificationRecipient(int userId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            if (currentUser.Role == Role.Administrator)
            {
                return true;
            }

            if (currentUser.Role == Role.TechnicalAdvisor)
            {
                return await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == userId || uns.User == currentUser)
                    .Select(uns => uns.OrganizationId)
                    .GroupBy(org => org)
                    .AnyAsync(g => g.Count() > 1);
            }

            var userOrganizations = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == userId || uns.User == currentUser)
                .Select(uns => uns.OrganizationId)
                .Distinct()
                .ToListAsync();

            return userOrganizations.Count == 1;
        }

        public bool HasCurrentUserAccessToAssignOrganizationToProject() =>
            _authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator);

        private async Task<bool> HasSupervisorAccessToProject(string supervisorIdentityName, int projectId) =>
            await _nyssContext.SupervisorUserProjects.FilterAvailableUsers().AnyAsync(sup => sup.SupervisorUser.EmailAddress == supervisorIdentityName && sup.ProjectId == projectId);

        private async Task<bool> HasHeadSupervisorAccessToProject(string headSupervisorIdentityName, int projectId) =>
            await _nyssContext.HeadSupervisorUserProjects.FilterAvailableUsers().AnyAsync(sup => sup.HeadSupervisorUser.EmailAddress == headSupervisorIdentityName && sup.ProjectId == projectId);
    }
}
