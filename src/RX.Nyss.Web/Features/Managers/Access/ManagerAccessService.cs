using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Managers.Access
{
    public interface IManagerAccessService
    {
        Task<bool> HasCurrentUserAccessToManager(int managerId);
    }

    public class ManagerAccessService : IManagerAccessService
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INyssContext _nyssContext;

        public ManagerAccessService(INationalSocietyAccessService nationalSocietyAccessService, IAuthorizationService authorizationService, INyssContext nyssContext)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
            _authorizationService = authorizationService;
            _nyssContext = nyssContext;
        }

        public async Task<bool> HasCurrentUserAccessToManager(int managerId)
        {
            if (!await _nationalSocietyAccessService.HasCurrentUserAccessToAnyNationalSocietiesOfGivenUser(managerId))
            {
                return false;
            }

            if (_authorizationService.IsCurrentUserInRole(Role.GlobalCoordinator))
            {
                var nationalSocietyHasCoordinator = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == managerId)
                    .Where(uns => uns.NationalSociety.NationalSocietyUsers.Any(x => x.User.Role == Role.Coordinator))
                    .AnyAsync();

                return !nationalSocietyHasCoordinator;
            }

            if (_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                var isHeadManager = await _nyssContext.Organizations
                    .Where(o => o.HeadManagerId == managerId || o.PendingHeadManagerId == managerId)
                    .AnyAsync();

                return isHeadManager;
            }

            return true;
        }
    }
}
