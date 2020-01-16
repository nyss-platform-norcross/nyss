using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.SmsGateway.Access
{
    public class SmsGatewayAccessHandler : ResourceAccessHandler<SmsGatewayAccessHandler>
    {
        private readonly ISmsGatewayAccessService _smsGatewayAccessService;

        public SmsGatewayAccessHandler(IHttpContextAccessor httpContextAccessor, ISmsGatewayAccessService smsGatewayAccessService)
            : base("smsGatewayId", httpContextAccessor)
        {
            _smsGatewayAccessService = smsGatewayAccessService;
        }

        protected override Task<bool> HasAccess(int smsGatewayId) =>
            _smsGatewayAccessService.HasCurrentUserAccessToSmsGateway(smsGatewayId);
    }
}
