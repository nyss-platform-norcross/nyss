using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Services
{
    public class DeleteUserServiceTests
    {
        private readonly IDeleteUserService _deleteUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INyssContext _nyssContext;

        public DeleteUserServiceTests()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _nyssContext = Substitute.For<INyssContext>();
            _deleteUserService = new DeleteUserService(_httpContextAccessor, _nyssContext);
        }

        [Theory]
        [InlineData(Role.DataConsumer)]
        [InlineData(Role.TechnicalAdvisor)]
        [InlineData(Role.Manager)]
        [InlineData(Role.GlobalCoordinator)]
        [InlineData(Role.Supervisor)]
        public void EnsureHasPermissionsToDelteUser_WhenSupervisorDeletesAnyOtherUser_ShouldThrowException(Role deletedUserRole)
        {
            //assert
            Should.Throw<ResultException>(() => _deleteUserService.EnsureHasPermissionsToDelteUser(deletedUserRole, new List<string> { Role.Supervisor.ToString() }))
                .Result.Message.Key.ShouldBe(ResultKey.User.Deletion.NoPermissionsToDeleteThisUser);
        }

        [Theory]
        [InlineData(Role.DataConsumer)]
        [InlineData(Role.TechnicalAdvisor)]
        [InlineData(Role.Manager)]
        [InlineData(Role.GlobalCoordinator)]
        [InlineData(Role.Supervisor)]
        public void EnsureHasPermissionsToDelteUser_WhenDataConsumerDeletesAnyOtherUser_ShouldThrowException(Role deletedUserRole)
        {
            //assert
            Should.Throw<ResultException>(() => _deleteUserService.EnsureHasPermissionsToDelteUser(deletedUserRole, new List<string> { Role.DataConsumer.ToString() }))
                .Result.Message.Key.ShouldBe(ResultKey.User.Deletion.NoPermissionsToDeleteThisUser);
        }

        [Theory]
        [InlineData(Role.Manager, Role.Supervisor)]
        [InlineData(Role.Manager, Role.Manager)]
        [InlineData(Role.Manager, Role.DataConsumer)]
        [InlineData(Role.Manager, Role.TechnicalAdvisor)]
        [InlineData(Role.TechnicalAdvisor, Role.Supervisor)]
        [InlineData(Role.TechnicalAdvisor, Role.Manager)]
        [InlineData(Role.TechnicalAdvisor, Role.DataConsumer)]
        [InlineData(Role.TechnicalAdvisor, Role.TechnicalAdvisor)]
        [InlineData(Role.GlobalCoordinator, Role.TechnicalAdvisor)]
        [InlineData(Role.GlobalCoordinator, Role.Manager)]
        [InlineData(Role.GlobalCoordinator, Role.DataConsumer)]
        [InlineData(Role.GlobalCoordinator, Role.GlobalCoordinator)]
        public void EnsureHasPermissionsToDelteUser_WhenManagerLevelUserOrGlobalCoordinatorDeletesUsersEqualOrBelowInHierarchy_ShouldNotThrow(Role deletingUserRole, Role deletedUserRole)
        {
            //assert
            Should.NotThrow(() => _deleteUserService.EnsureHasPermissionsToDelteUser(deletedUserRole, new List<string> { deletingUserRole.ToString() }));
        }

        [Theory]
        [InlineData(Role.Administrator, Role.Administrator)]
        [InlineData(Role.Administrator, Role.GlobalCoordinator)]
        [InlineData(Role.Administrator, Role.TechnicalAdvisor)]
        [InlineData(Role.Administrator, Role.Manager)]
        [InlineData(Role.Administrator, Role.DataConsumer)]
        [InlineData(Role.Administrator, Role.Supervisor)]
        public void EnsureHasPermissionsToDelteUser_WhenAdministratorDeletesUSer_ShouldNotThrow(Role deletingUserRole, Role deletedUserRole)
        {
            //assert
            Should.NotThrow(() => _deleteUserService.EnsureHasPermissionsToDelteUser(deletedUserRole, new List<string> { deletingUserRole.ToString() }));
        }

        [Theory]
        [InlineData(Role.Manager, Role.GlobalCoordinator)]
        [InlineData(Role.Manager, Role.Administrator)]
        [InlineData(Role.TechnicalAdvisor, Role.GlobalCoordinator)]
        [InlineData(Role.TechnicalAdvisor, Role.Administrator)]
        [InlineData(Role.GlobalCoordinator, Role.Administrator)]
        public void EnsureHasPermissionsToDelteUser_WhenManagerLevelUserOrGlobalCoordinatorDeletesUsersHigherInHierarchy_ShouldThrow(Role deletingUserRole, Role deletedUserRole)
        {
            //assert
            Should.Throw<ResultException>(() => _deleteUserService.EnsureHasPermissionsToDelteUser(deletedUserRole, new List<string> { deletingUserRole.ToString() }))
                .Result.Message.Key.ShouldBe(ResultKey.User.Deletion.NoPermissionsToDeleteThisUser);
        }
    }
}
