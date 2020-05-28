using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocieties.Access
{
    public class NationalSocietyAccessHandler : ResourceAccessHandler<NationalSocietyAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public NationalSocietyAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("nationalSocietyId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int nationalSocietyId, bool readOnly) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(nationalSocietyId);
    }
}
