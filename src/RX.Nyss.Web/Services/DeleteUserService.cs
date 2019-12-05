using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Services
{
    public interface IDeleteUserService
    {
        void EnsureHasPermissionsToDelteUser(Role deletedUserRole, IEnumerable<string> deletingUserRoles);
    }

    public class DeleteUserService: IDeleteUserService
    {
        private readonly IDictionary<string, int> _userRoleHierarchyDictionary = new Dictionary<string, int>();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private INyssContext _nyssContext;

        public DeleteUserService(IHttpContextAccessor httpContextAccessor, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
            SetupUserRolesHierarchy();
        }

        private void SetupUserRolesHierarchy()
        {
            _userRoleHierarchyDictionary[Role.Administrator.ToString()] = 1;
            _userRoleHierarchyDictionary[Role.GlobalCoordinator.ToString()] = 2;
            _userRoleHierarchyDictionary[Role.DataConsumer.ToString()] = 3;
            _userRoleHierarchyDictionary[Role.Manager.ToString()] = 3;
            _userRoleHierarchyDictionary[Role.TechnicalAdvisor.ToString()] = 3;
            _userRoleHierarchyDictionary[Role.Supervisor.ToString()] = 4;
        }

        public void EnsureHasPermissionsToDelteUser(Role deletedUserRole, IEnumerable<string> deletingUserRoles)
        {
            var deletingUserRole = deletingUserRoles.Single();
            if (deletingUserRole == Role.Supervisor.ToString()
                || deletingUserRole == Role.DataConsumer.ToString()
                || _userRoleHierarchyDictionary[deletingUserRole] > _userRoleHierarchyDictionary[deletedUserRole.ToString()])
            {
                throw new ResultException(ResultKey.User.Deletion.NoPermissionsToDeleteThisUser);
            }
        }
    }
}
