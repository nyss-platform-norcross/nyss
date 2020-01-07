using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSociety.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.TechnicalAdvisor.Access
{
    public class TechnicalAdvisorAccessHandler : ResourceAccessHandler<TechnicalAdvisorAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public TechnicalAdvisorAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("technicalAdvisorId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int technicalAdvisorId) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToUserNationalSocieties(technicalAdvisorId);
    }
}
