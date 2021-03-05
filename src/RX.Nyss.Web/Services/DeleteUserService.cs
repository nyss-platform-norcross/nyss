using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Services.Authorization;

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
        private readonly IAuthorizationService _authorizationService;
        private readonly INyssContext _nyssContext;

        public DeleteUserService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            SetupUserRolesHierarchy();
        }

        public async Task EnsureCanDeleteUser(int deletedUserId, Role deletedUserRole)
        {
            var callingUserEmail = _authorizationService.GetCurrentUserName();
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
            var cannotDeleteAnyone = deletingUserRole == Role.HeadSupervisor || deletingUserRole == Role.Supervisor || deletingUserRole == Role.DataConsumer;

            if (cannotDeleteAnyone || _userRoleHierarchyDictionary[deletingUserRole] > _userRoleHierarchyDictionary[deletedUserRole])
            {
                return false;
            }

            return true;
        }

        private void SetupUserRolesHierarchy()
        {
            _userRoleHierarchyDictionary[Role.Administrator] = 1;
            _userRoleHierarchyDictionary[Role.GlobalCoordinator] = 2;
            _userRoleHierarchyDictionary[Role.Coordinator] = 3;
            _userRoleHierarchyDictionary[Role.DataConsumer] = 4;
            _userRoleHierarchyDictionary[Role.Manager] = 4;
            _userRoleHierarchyDictionary[Role.TechnicalAdvisor] = 4;
            _userRoleHierarchyDictionary[Role.Supervisor] = 5;
            _userRoleHierarchyDictionary[Role.HeadSupervisor] = 5;
        }
    }
}
