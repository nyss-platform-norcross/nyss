using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.NationalSocieties.Access
{
    public interface INationalSocietyAccessService
    {
        bool HasCurrentUserAccessToAllNationalSocieties();
        Task<bool> HasCurrentUserAccessToAnyNationalSocietiesOfGivenUser(int userId);
        Task<bool> HasCurrentUserAccessToNationalSociety(int nationalSocietyId);
        Task<bool> HasCurrentUserAccessAsHeadManager(int organizationId);
        Task<bool> HasCurrentUserAccessToModifyNationalSociety(int nationalSocietyId);
    }

    public class NationalSocietyAccessService : INationalSocietyAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public NationalSocietyAccessService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public bool HasCurrentUserAccessToAllNationalSocieties() =>
            // ToDo: Global Coordinator does not have permissions to SMS Gateways, Geographical structure, Projects, etc. but Head Manager does. Investigate and fix later
            _authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.GlobalCoordinator);

        public async Task<bool> HasCurrentUserAccessToAnyNationalSocietiesOfGivenUser(int userId)
        {
            if (HasCurrentUserAccessToAllNationalSocieties())
            {
                return true;
            }

            var currentUserName = _authorizationService.GetCurrentUserName();

            return await _nyssContext.UserNationalSocieties
                .AnyAsync(uns1 => uns1.UserId == userId
                    && !(uns1.User is DataConsumerUser && uns1.User.IsFirstLogin)
                    && uns1.NationalSociety.NationalSocietyUsers.Any(uns2 => uns2.User.EmailAddress == currentUserName));
        }

        public async Task<bool> HasCurrentUserAccessToNationalSociety(int nationalSocietyId)
        {
            if (HasCurrentUserAccessToAllNationalSocieties())
            {
                return true;
            }

            var userName = _authorizationService.GetCurrentUserName();

            return await _nyssContext.UserNationalSocieties
                .Where(u => u.User.EmailAddress == userName)
                .AnyAsync(uns => uns.NationalSocietyId == nationalSocietyId);
        }

        public async Task<bool> HasCurrentUserAccessAsHeadManager(int organizationId)
        {
            if (!_authorizationService.IsCurrentUserInAnyRole(Role.Manager, Role.TechnicalAdvisor))
            {
                return true;
            }

            var userName = _authorizationService.GetCurrentUserName();

            return await _nyssContext.Organizations
                .AnyAsync(o => o.Id == organizationId && o.HeadManager.EmailAddress == userName);
        }

        public async Task<bool> HasCurrentUserAccessToModifyNationalSociety(int nationalSocietyId)
        {
            if (HasCurrentUserAccessToAllNationalSocieties())
            {
                return true;
            }

            var userName = _authorizationService.GetCurrentUserName();

            var nationalSocietyData = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    HasCurrentUserAccessToNationalSociety = ns.NationalSocietyUsers.Any(nsu => nsu.User.EmailAddress == userName),
                    CurrentUserOrganizationId = ns.NationalSocietyUsers
                        .Where(uns => uns.User.EmailAddress == userName)
                        .Select(uns => uns.OrganizationId)
                        .SingleOrDefault(),
                    HasCoordinator = ns.NationalSocietyUsers
                        .Any(uns => uns.User.Role == Role.Coordinator)
                })
                .SingleAsync();

            return nationalSocietyData.HasCurrentUserAccessToNationalSociety
                && (!nationalSocietyData.HasCoordinator || _authorizationService.IsCurrentUserInAnyRole(Role.Coordinator));
        }
    }
}
