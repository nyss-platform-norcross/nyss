using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSociety.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.User.Access
{
    public class HeadManagerAccessHandler : ResourceAccessHandler<HeadManagerAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public HeadManagerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("nationalSocietyId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int nationalSocietyId) =>
            _nationalSocietyAccessService.HasCurrentUserAccessAsHeadManager(nationalSocietyId);
    }
}
