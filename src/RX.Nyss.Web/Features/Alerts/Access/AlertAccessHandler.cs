using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Alerts.Access
{
    public class AlertAccessHandler : ResourceAccessHandler<AlertAccessHandler>
    {
        private readonly IAlertAccessService _alertAccessService;

        public AlertAccessHandler(IHttpContextAccessor httpContextAccessor, IAlertAccessService alertAccessService)
            : base("alertId", httpContextAccessor)
        {
            _alertAccessService = alertAccessService;
        }

        protected override Task<bool> HasAccess(int alertId, bool readOnly) =>
            _alertAccessService.HasCurrentUserAccessToAlert(alertId);
    }
}
