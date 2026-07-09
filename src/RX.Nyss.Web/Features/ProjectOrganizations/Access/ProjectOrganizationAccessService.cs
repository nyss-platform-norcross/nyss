using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Projects.Access;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Access
{
    public interface IProjectOrganizationAccessService
    {
        Task<bool> HasCurrentUserAccessToProjectOrganization(int projectOrganizationId);
    }

    public class ProjectOrganizationAccessService : IProjectOrganizationAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IProjectAccessService _projectAccessService;

        public ProjectOrganizationAccessService(
            INyssContext nyssContext,
            IProjectAccessService projectAccessService)
        {
            _nyssContext = nyssContext;
            _projectAccessService = projectAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToProjectOrganization(int projectOrganizationId)
        {
            var projectId = await _nyssContext.ProjectOrganizations
                .Where(g => g.Id == projectOrganizationId)
                .Select(s => s.Project.Id)
                .SingleAsync();

            return await _projectAccessService.HasCurrentUserAccessToProject(projectId);
        }
    }
}
