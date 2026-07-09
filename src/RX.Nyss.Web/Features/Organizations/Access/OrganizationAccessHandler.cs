using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Organizations.Access
{
    public class OrganizationAccessHandler : ResourceAccessHandler<OrganizationAccessHandler>
    {
        private readonly IOrganizationAccessService _organizationAccessService;

        public OrganizationAccessHandler(IHttpContextAccessor httpContextAccessor, IOrganizationAccessService organizationAccessService)
            : base("organizationId", httpContextAccessor)
        {
            _organizationAccessService = organizationAccessService;
        }

        protected override Task<bool> HasAccess(int organizationId, bool readOnly) =>
            _organizationAccessService.HasCurrentUserAccessToOrganization(organizationId);
    }
}
