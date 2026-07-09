using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Managers.Access
{
    public class ManagerAccessHandler : ResourceAccessHandler<ManagerAccessHandler>
    {
        private readonly IManagerAccessService _managerAccessService;

        public ManagerAccessHandler(IHttpContextAccessor httpContextAccessor, IManagerAccessService managerAccessService)
            : base("managerId", httpContextAccessor)
        {
            _managerAccessService = managerAccessService;
        }

        protected override Task<bool> HasAccess(int managerId, bool readOnly) =>
            _managerAccessService.HasCurrentUserAccessToManager(managerId);
    }
}
