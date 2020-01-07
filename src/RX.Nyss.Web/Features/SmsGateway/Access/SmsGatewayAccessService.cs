using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSociety.Access;

namespace RX.Nyss.Web.Features.SmsGateway.Access
{
    public interface ISmsGatewayAccessService
    {
        Task<bool> HasCurrentUserAccessToSmsGateway(int smsGatewayId);
    }

    public class SmsGatewayAccessService : ISmsGatewayAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public SmsGatewayAccessService(
            INyssContext nyssContext,
            INationalSocietyAccessService nationalSocietyAccessService)
        {
            _nyssContext = nyssContext;
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToSmsGateway(int smsGatewayId)
        {
            var nationalSocietyId = await _nyssContext.GatewaySettings
                .Where(g => g.Id == smsGatewayId)
                .Select(s => s.NationalSociety.Id)
                .SingleAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }
    }
}
