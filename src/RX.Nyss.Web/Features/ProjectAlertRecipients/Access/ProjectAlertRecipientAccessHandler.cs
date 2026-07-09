using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Access
{
    public class ProjectAlertRecipientAccessHandler : ResourceAccessHandler<ProjectAlertRecipientAccessHandler>
    {
        private readonly IProjectAlertRecipientAccessService _alertRecipientAccessService;

        public ProjectAlertRecipientAccessHandler(IHttpContextAccessor httpContextAccessor, IProjectAlertRecipientAccessService AlertRecipientAccessService)
            : base("alertRecipientId", httpContextAccessor)
        {
            _alertRecipientAccessService = AlertRecipientAccessService;
        }

        protected override Task<bool> HasAccess(int alertRecipientId, bool readOnly) =>
            _alertRecipientAccessService.HasCurrentUserAccessToAlertRecipients(alertRecipientId);
    }
}
