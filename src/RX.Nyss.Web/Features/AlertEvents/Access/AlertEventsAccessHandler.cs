using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.AlertEvents.Access
{
    public class AlertEventsAccessHandler : ResourceAccessHandler<AlertEventsAccessHandler>
    {
        private readonly IAlertEventsAccessService _alertEventsAccessService;

        public AlertEventsAccessHandler(IHttpContextAccessor httpContextAccessor, IAlertEventsAccessService alertEventAccessService)
            : base("alertId", httpContextAccessor)
        {
            _alertEventsAccessService = alertEventAccessService;
        }

        protected override Task<bool> HasAccess(int alertId, bool readOnly) =>
            _alertEventsAccessService.HasCurrentUserAccessToAlert(alertId);
    }
}
