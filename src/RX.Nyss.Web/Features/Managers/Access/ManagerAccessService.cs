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
            if (!await _nationalSocietyAccessService.HasCurrentUserAccessToUserNationalSocieties(managerId))
            {
                return false;
            } 

            if (_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                var isHeadManager = await _nyssContext.NationalSocieties.Where(ns => ns.HeadManager.Id == managerId || ns.PendingHeadManager.Id == managerId).AnyAsync();
                return isHeadManager;
            }

            return true;
        }
    }
}
