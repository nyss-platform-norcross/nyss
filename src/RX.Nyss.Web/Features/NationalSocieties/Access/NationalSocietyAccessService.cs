using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.NationalSocieties.Access
{
    public interface INationalSocietyAccessService
    {
        bool HasCurrentUserAccessToAllNationalSocieties();
        Task<bool> HasCurrentUserAccessToUserNationalSocieties(int userId);
        Task<bool> HasCurrentUserAccessToNationalSocieties(IEnumerable<int> providedNationalSocietyIds);
        Task<bool> HasCurrentUserAccessAsHeadManager(int organizationId);
        Task<List<int>> GetCurrentUserNationalSocietyIds();
    }

    public class NationalSocietyAccessService : INationalSocietyAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly Role[] _rolesWithAccessToAllNationalSocieties;
        private readonly Role[] _rolesWithAccessAsHeadManager;

        public NationalSocietyAccessService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;

            _rolesWithAccessToAllNationalSocieties = new[] { Role.Administrator, Role.GlobalCoordinator };
            // ToDo: Global Coordinator does not have permissions to SMS Gateways, Geographical structure, Projects, etc. but Head Manager does. Investigate and fix later
            _rolesWithAccessAsHeadManager = new[] { Role.GlobalCoordinator, Role.Administrator };
        }

        public bool HasCurrentUserAccessToAllNationalSocieties() =>
            _authorizationService.IsCurrentUserInAnyRole(_rolesWithAccessToAllNationalSocieties);

        public async Task<bool> HasCurrentUserAccessToUserNationalSocieties(int userId)
        {
            var userNationalSocieties = await _nyssContext.UserNationalSocieties.FilterAvailableUsers()
                .Where(u => u.UserId == userId)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();

            return await HasCurrentUserAccessToNationalSocieties(userNationalSocieties);
        }

        public async Task<bool> HasCurrentUserAccessToNationalSocieties(IEnumerable<int> providedNationalSocietyIds)
        {
            if (HasCurrentUserAccessToAllNationalSocieties())
            {
                return true;
            }

            var userNationalSocieties = await GetCurrentUserNationalSocietyIds();
            return providedNationalSocietyIds.Intersect(userNationalSocieties).Any();
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

        public async Task<List<int>> GetCurrentUserNationalSocietyIds()
        {
            var userName = _authorizationService.GetCurrentUserName();

            return await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userName)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();
        }
    }
}
