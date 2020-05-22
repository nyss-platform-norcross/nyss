using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.SmsGateways.Access
{
    public interface ISmsGatewayAccessService
    {
        Task<bool> HasCurrentUserAccessToSmsGateway(int smsGatewayId);
    }

    public class SmsGatewayAccessService : ISmsGatewayAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public SmsGatewayAccessService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            INationalSocietyAccessService nationalSocietyAccessService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToSmsGateway(int smsGatewayId)
        {
            var currentUser = _authorizationService.GetCurrentUser();

            var nationalSocietyData = await _nyssContext.GatewaySettings
                .Where(g => g.Id == smsGatewayId)
                .Select(ns => new
                {
                    CurrentUserOrganizationId = ns.NationalSociety.NationalSocietyUsers
                        .Where(uns => uns.User == currentUser)
                        .Select(uns => uns.OrganizationId)
                        .SingleOrDefault(),
                    HasCoordinator = ns.NationalSociety.NationalSocietyUsers
                        .Any(uns => uns.User.Role == Role.Coordinator)
                })
                .SingleAsync();

            if (nationalSocietyData.HasCoordinator && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
            {
                return false;
            }

            var nationalSocietyId = await _nyssContext.GatewaySettings
                .Where(g => g.Id == smsGatewayId)
                .Select(s => s.NationalSociety.Id)
                .SingleAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(nationalSocietyId);
        }
    }
}
