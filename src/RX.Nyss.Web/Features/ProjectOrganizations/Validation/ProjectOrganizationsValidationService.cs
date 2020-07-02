using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Validation
{
    public interface IProjectOrganizationValidationService
    {
        Task<bool> HasSameOrganization(int organizationId);
    }

    public class ProjectOrganizationValidationService : IProjectOrganizationValidationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ProjectIdRouteParameterName = "projectId";

        public ProjectOrganizationValidationService(INyssContext nyssContext, IHttpContextAccessor httpContextAccessor)
        {
            _nyssContext = nyssContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasSameOrganization(int organizationId)
        {
            var projectId = _httpContextAccessor.GetResourceParameter(ProjectIdRouteParameterName);

            return await _nyssContext.ProjectOrganizations
                .AnyAsync(po => po.ProjectId == projectId && po.OrganizationId == organizationId);
        }
    }
}
