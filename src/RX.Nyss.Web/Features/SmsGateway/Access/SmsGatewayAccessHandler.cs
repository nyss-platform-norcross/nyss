using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.SmsGateway.Access
{
    public class SmsGatewayAccessHandler : ResourceAccessHandler<SmsGatewayAccessHandler>
    {
        private readonly ISmsGatewayAccessService _projectAccessService;

        public SmsGatewayAccessHandler(IHttpContextAccessor httpContextAccessor, ISmsGatewayAccessService projectAccessService)
            : base("smsGatewayId", httpContextAccessor)
        {
            _projectAccessService = projectAccessService;
        }

        protected override Task<bool> HasAccess(int smsGatewayId) =>
            _projectAccessService.HasCurrentUserAccessToSmsGateway(smsGatewayId);
    }
}
