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

            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }

        public bool HasCurrentUserAccessToAssignOrganizationToProject() =>
            _authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator);

        private async Task<bool> HasSupervisorAccessToProject(string supervisorIdentityName, int projectId) =>
            await _nyssContext.SupervisorUserProjects.FilterAvailableUsers().AnyAsync(sup => sup.SupervisorUser.EmailAddress == supervisorIdentityName && sup.ProjectId == projectId);
    }
}
