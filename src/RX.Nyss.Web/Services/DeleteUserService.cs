using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Queries;

namespace RX.Nyss.Web.Services
{
    public interface IDeleteUserService
    {
        Task EnsureCanDeleteUser(int deletedUserId, Role deletedUserRole);
        bool GetCanRoleDeleteRole(Role deletedUserRole, Role deletingUserRole);
    }

    public class DeleteUserService : IDeleteUserService
    {
        private readonly IDictionary<Role, int> _userRoleHierarchyDictionary = new Dictionary<Role, int>();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INyssContext _nyssContext;

        public DeleteUserService(IHttpContextAccessor httpContextAccessor, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
            SetupUserRolesHierarchy();
        }

        public async Task EnsureCanDeleteUser(int deletedUserId, Role deletedUserRole)
        {
            var callingUserEmail = _httpContextAccessor.HttpContext.User.Identity.Name;
            var callingUserData = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == callingUserEmail)
                .Select(u => new
                {
                    u.Id,
                    u.Role
                })
                .SingleAsync();

            if (deletedUserId == callingUserData.Id)
            {
                throw new ResultException(ResultKey.User.Deletion.CannotDeleteYourself);
            }

            var hasRolePermissions = GetCanRoleDeleteRole(deletedUserRole, callingUserData.Role);
            if (!hasRolePermissions)
            {
                throw new ResultException(ResultKey.User.Deletion.NoPermissionsToDeleteThisUser);
            }
        }

        public bool GetCanRoleDeleteRole(Role deletedUserRole, Role deletingUserRole)
        {
            if (deletingUserRole == Role.Supervisor
                || deletingUserRole == Role.DataConsumer
                || _userRoleHierarchyDictionary[deletingUserRole] > _userRoleHierarchyDictionary[deletedUserRole])
            {
                return false;
            }

            return true;
        }

        private void SetupUserRolesHierarchy()
        {
            _userRoleHierarchyDictionary[Role.Administrator] = 1;
            _userRoleHierarchyDictionary[Role.GlobalCoordinator] = 2;
            _userRoleHierarchyDictionary[Role.DataConsumer] = 3;
            _userRoleHierarchyDictionary[Role.Manager] = 3;
            _userRoleHierarchyDictionary[Role.TechnicalAdvisor] = 3;
            _userRoleHierarchyDictionary[Role.Supervisor] = 4;
        }
    }
}
