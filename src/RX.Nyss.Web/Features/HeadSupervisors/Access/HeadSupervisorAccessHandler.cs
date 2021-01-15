using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.HeadSupervisors.Access
{
    public class HeadSupervisorAccessHandler : ResourceAccessHandler<HeadSupervisorAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public HeadSupervisorAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("headSupervisorId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int headSupervisorId, bool readOnly) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToAnyNationalSocietiesOfGivenUser(headSupervisorId);
    }
}
