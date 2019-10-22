using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Logging;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils.DataContract;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.User
{
    public class UserServiceTests
    {
        private readonly ILoggerAdapter _loggerAdapterMock;
        private readonly INyssContext _nyssContextMock;

        public UserServiceTests()
        {
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _nyssContextMock = Substitute.For<INyssContext>();
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenEmptyUserList_ShouldReturnSuccessfulResult()
        {
            var userEmail = "emailTest1@domain.com";
            var existingUserList = new List<IdentityUser>();
            var userService = GetUserServiceWithMockedDependencies(existingUserList);
            var globalManagerInDto = new GlobalCoordinatorInDto { Name = userEmail, Email = userEmail };

            var result = await userService.RegisterGlobalCoordinator(globalManagerInDto);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenEmptyUserList_NyssContextAddAsyncIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var existingUserList = new List<IdentityUser>();
            var userService = GetUserServiceWithMockedDependencies(existingUserList);
            var globalManagerInDto = new GlobalCoordinatorInDto { Name = userEmail, Email = userEmail };

            await userService.RegisterGlobalCoordinator(globalManagerInDto);

            await _nyssContextMock.Received().AddAsync(Arg.Any<GlobalCoordinatorUser>());
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenEmptyUserList_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var existingUserList = new List<IdentityUser>();
            var userService = GetUserServiceWithMockedDependencies(existingUserList);
            var globalManagerInDto = new GlobalCoordinatorInDto { Name = userEmail, Email = userEmail };

            await userService.RegisterGlobalCoordinator(globalManagerInDto);

            await _nyssContextMock.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenUserAlreadyExists_ShouldReturnUnsuccessfulResultWithMessageKey()
        {
            var userEmail = "emailTest1@domain.com";
            var existingUserList = new List<IdentityUser> { new IdentityUser(){ UserName = userEmail , Email = userEmail } };
            var userService = GetUserServiceWithMockedDependencies(existingUserList);
            var globalManagerInDto = new GlobalCoordinatorInDto { Name = userEmail, Email = userEmail };

            var result = await userService.RegisterGlobalCoordinator(globalManagerInDto);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultKey.User.Registration.UserAlreadyExists, result.Message.Key);
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_UnknownErrorExceptionThrown_ShouldReturnUnsuccessfulResultWithMessageKey()
        {
            var userEmail = "emailTest1@domain.com";

            _nyssContextMock.When(c => c.SaveChangesAsync())
                .Do(x => throw new ResultException(ResultKey.User.Registration.UnknownError));

            var existingUserList = new List<IdentityUser>();
            var userService = GetUserServiceWithMockedDependencies(existingUserList);
            var globalManagerInDto = new GlobalCoordinatorInDto { Name = userEmail, Email = userEmail };

            var result = await userService.RegisterGlobalCoordinator(globalManagerInDto);

            Assert.False(result.IsSuccess);
            Assert.Equal(ResultKey.User.Registration.UnknownError, result.Message.Key);
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_BaseExceptionThrown_ShouldPassThrougWithoutBeingCaught()
        {
            var userEmail = "emailTest1@domain.com";

            _nyssContextMock.When(c => c.SaveChangesAsync())
                .Do(x => throw new Exception());

            var existingUserList = new List<IdentityUser>();
            var userService = GetUserServiceWithMockedDependencies(existingUserList);
            var globalManagerInDto = new GlobalCoordinatorInDto { Name = userEmail, Email = userEmail };

            await Assert.ThrowsAsync<Exception>(() => userService.RegisterGlobalCoordinator(globalManagerInDto));
        }

        private UserService GetUserServiceWithMockedDependencies(List<IdentityUser> users)
        {
            var userManager = MockUserManager(users);
            
            var userService = new UserService(userManager, _loggerAdapterMock, _nyssContextMock);
            return userService;
        }

        public UserManager<IdentityUser> MockUserManager(List<IdentityUser> users)
        {
            var store = MockUserEmailStore(users);

            var manager = new UserManager<IdentityUser>(store, null, null, null, null, null, null, null, null);

            var userValidator = Substitute.For<IUserValidator<IdentityUser>>();
            userValidator.ValidateAsync(manager, Arg.Any<IdentityUser>()).Returns(IdentityResult.Success);
            manager.UserValidators.Add(userValidator);
            manager.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
            manager.Logger = Substitute.For<ILogger>();

            return manager;
        }

        private IUserEmailStore<IdentityUser> MockUserEmailStore(List<IdentityUser> users)
        {
            var store = Substitute.For<IUserEmailStore<IdentityUser>, IUserRoleStore<IdentityUser>>();
            
            store.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(ci => users.FirstOrDefault(u => u.Email == ci.Arg<string>()));

            store.CreateAsync(Arg.Any<IdentityUser>(), Arg.Any<CancellationToken>()).Returns(IdentityResult.Success);
            store.When(s => s.CreateAsync(Arg.Any<IdentityUser>(), Arg.Any<CancellationToken>()))
                .Do(ci => { users.Add(ci.Arg<IdentityUser>()); });

            ((IUserRoleStore<IdentityUser>) store).UpdateAsync(Arg.Any<IdentityUser>(), Arg.Any<CancellationToken>())
                .Returns(IdentityResult.Success);

            return store;
        }
    }
}
