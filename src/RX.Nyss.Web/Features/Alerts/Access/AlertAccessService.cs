using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Projects.Access;

namespace RX.Nyss.Web.Features.Alerts.Access
{
    public interface IAlertAccessService
    {
        Task<bool> HasCurrentUserAccessToAlert(int alertId);
    }

    public class AlertAccessService : IAlertAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IProjectAccessService _projectAccessService;

        public AlertAccessService(
            INyssContext nyssContext,
            IProjectAccessService projectAccessService)
        {
            _nyssContext = nyssContext;
            _projectAccessService = projectAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToAlert(int alertId)
        {
            var projectId = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => a.ProjectHealthRisk.Project.Id)
                .SingleAsync();

            return await _projectAccessService.HasCurrentUserAccessToProject(projectId);
        }
    }
}
