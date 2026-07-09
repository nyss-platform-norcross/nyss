using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Users.Access
{
    public class HeadManagerAccessHandler : ResourceAccessHandler<HeadManagerAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public HeadManagerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("organizationId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int organizationId, bool readOnly) =>
            _nationalSocietyAccessService.HasCurrentUserAccessAsHeadManager(organizationId);
    }
}
