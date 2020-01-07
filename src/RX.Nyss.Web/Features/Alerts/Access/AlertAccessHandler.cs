using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Alerts.Access
{
    public class AlertAccessHandler : ResourceAccessHandler<AlertAccessHandler>
    {
        private readonly IAlertAccessService _projectAccessService;

        public AlertAccessHandler(IHttpContextAccessor httpContextAccessor, IAlertAccessService projectAccessService)
            : base("alertId", httpContextAccessor)
        {
            _projectAccessService = projectAccessService;
        }

        protected override Task<bool> HasAccess(int alertId) =>
            _projectAccessService.HasCurrentUserAccessToAlert(alertId);
    }
}
