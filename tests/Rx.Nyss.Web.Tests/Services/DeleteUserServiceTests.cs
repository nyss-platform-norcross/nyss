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
        public void GetCanRoleDeleteRole_WhenSupervisorDeletesAnyOtherUser_ShouldReturnFalse(Role deletedUserRole)
        {
            //Act
            var result = _deleteUserService.GetCanRoleDeleteRole(deletedUserRole, Role.Supervisor);

            //assert
            result.ShouldBeFalse();
        }

        [Theory]
        [InlineData(Role.DataConsumer)]
        [InlineData(Role.TechnicalAdvisor)]
        [InlineData(Role.Manager)]
        [InlineData(Role.GlobalCoordinator)]
        [InlineData(Role.Supervisor)]
        public void GetCanRoleDeleteRole_WhenDataConsumerDeletesAnyOtherUser_ShouldReturnFalse(Role deletedUserRole)
        {
            //Act
            var result = _deleteUserService.GetCanRoleDeleteRole(deletedUserRole, Role.DataConsumer);

            //assert
            result.ShouldBeFalse();
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
        public void GetCanRoleDeleteRole_WhenManagerLevelUserOrGlobalCoordinatorDeletesUsersEqualOrBelowInHierarchy_ShouldReturnSuccess(Role deletingUserRole, Role deletedUserRole)
        {
            //Act
            var result = _deleteUserService.GetCanRoleDeleteRole(deletedUserRole, deletingUserRole);

            //assert
            result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(Role.Administrator, Role.Administrator)]
        [InlineData(Role.Administrator, Role.GlobalCoordinator)]
        [InlineData(Role.Administrator, Role.TechnicalAdvisor)]
        [InlineData(Role.Administrator, Role.Manager)]
        [InlineData(Role.Administrator, Role.DataConsumer)]
        [InlineData(Role.Administrator, Role.Supervisor)]
        public void GetCanRoleDeleteRole_WhenAdministratorDeletesUSer_ShouldReturnSuccess(Role deletingUserRole, Role deletedUserRole)
        {
            //Act
            var result = _deleteUserService.GetCanRoleDeleteRole(deletedUserRole, deletingUserRole);

            //assert
            result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(Role.Manager, Role.GlobalCoordinator)]
        [InlineData(Role.Manager, Role.Administrator)]
        [InlineData(Role.TechnicalAdvisor, Role.GlobalCoordinator)]
        [InlineData(Role.TechnicalAdvisor, Role.Administrator)]
        [InlineData(Role.GlobalCoordinator, Role.Administrator)]
        public void GetCanRoleDeleteRole_WhenManagerLevelUserOrGlobalCoordinatorDeletesUsersHigherInHierarchy_ShouldReturnFalse(Role deletingUserRole, Role deletedUserRole)
        {
            //Act
            var result = _deleteUserService.GetCanRoleDeleteRole(deletedUserRole, deletingUserRole);

            //assert
            result.ShouldBeFalse();
        }
    }
}
