using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.Administration.GlobalCoordinator
{
    public class GlobalCoordinatorServiceTests
    {
        private readonly ILoggerAdapter _loggerAdapterMock;
        private readonly INyssContext _nyssContextMock;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly GlobalCoordinatorService _globalCoordinatorService;

        public GlobalCoordinatorServiceTests()
        {
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _nyssContextMock = Substitute.For<INyssContext>();
            _identityUserRegistrationService = Substitute.For<IIdentityUserRegistrationService>();
            _globalCoordinatorService = new GlobalCoordinatorService(_identityUserRegistrationService, _nyssContextMock, _loggerAdapterMock);

            _identityUserRegistrationService.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new RegisterGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            var (result, _) = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new RegisterGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            var result = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            await _nyssContextMock.Received().AddAsync(Arg.Any<GlobalCoordinatorUser>());
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new RegisterGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            var result = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            await _nyssContextMock.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationService.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new RegisterGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };
            

            var (result, _) = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            Assert.False(result.IsSuccess);
            Assert.Equal(exception.Result.Message.Key, result.Message.Key);
        }

        [Fact]
        public void RegisterGlobalCoordinator_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationService.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new RegisterGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };

            Assert.ThrowsAsync<Exception>(() => _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto));
        }
    }
}
