using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.DataConsumers.Access
{
    public class DataConsumerAccessHandler : ResourceAccessHandler<DataConsumerAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public DataConsumerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("dataConsumerId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int dataConsumerId) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToAnyNationalSocietiesOfGivenUser(dataConsumerId);
    }
}
