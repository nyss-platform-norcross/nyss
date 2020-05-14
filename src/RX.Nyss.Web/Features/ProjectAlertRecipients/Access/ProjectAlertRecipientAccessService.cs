using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Projects.Access;

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

        public ProjectAlertRecipientAccessService(
            INyssContext nyssContext,
            IProjectAccessService projectAccessService)
        {
            _nyssContext = nyssContext;
            _projectAccessService = projectAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToAlertRecipients(int alertRecipientId)
        {
            var projectId = await _nyssContext.AlertNotificationRecipients
                .Where(anr => anr.Id == alertRecipientId)
                .Select(anr => anr.ProjectId)
                .SingleAsync();

            return await _projectAccessService.HasCurrentUserAccessToProject(projectId);
        }
    }
}
