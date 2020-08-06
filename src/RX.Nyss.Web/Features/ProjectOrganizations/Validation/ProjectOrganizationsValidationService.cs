using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Validation
{
    public interface IProjectOrganizationValidationService
    {
        Task<bool> OrganizationAlreadyAddedToProject(int organizationId, int? projectId);
    }

    public class ProjectOrganizationValidationService : IProjectOrganizationValidationService
    {
        private readonly INyssContext _nyssContext;

        public ProjectOrganizationValidationService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<bool> OrganizationAlreadyAddedToProject(int organizationId, int? projectId) =>
            await _nyssContext.ProjectOrganizations
                .AnyAsync(po => po.ProjectId == projectId && po.OrganizationId == organizationId);
    }
}
