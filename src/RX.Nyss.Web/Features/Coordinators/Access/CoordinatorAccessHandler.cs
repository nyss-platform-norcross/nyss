using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Coordinators.Access
{
    public class CoordinatorAccessHandler : ResourceAccessHandler<CoordinatorAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public CoordinatorAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("coordinatorId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int coordinatorId, bool readOnly) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToAnyNationalSocietiesOfGivenUser(coordinatorId);
    }
}
