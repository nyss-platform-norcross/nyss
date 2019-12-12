using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.GlobalCoordinator;
using RX.Nyss.Web.Features.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.GlobalCoordinator
{
    public class GlobalCoordinatorServiceTests
    {
        private readonly GlobalCoordinatorService _globalCoordinatorService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly IVerificationEmailService _verificationEmailServiceMock;
        private IDeleteUserService _deleteUserService;

        public GlobalCoordinatorServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _deleteUserService = Substitute.For<IDeleteUserService>();

            _globalCoordinatorService = new GlobalCoordinatorService(_identityUserRegistrationServiceMock, _nyssContext, _loggerAdapter, _verificationEmailServiceMock, _deleteUserService);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });

            ArrangeApplicationLanguages();
        }

        private void ArrangeApplicationLanguages()
        {
            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);
        }

        private void ArrangeUsersDbSetWithOneAdministratorUser() =>
            ArrangeUsersDbSetWithExistingUsers(new List<Nyss.Data.Models.User> { new AdministratorUser() { Id = 123, Role = Role.Administrator } });

        private void ArrangeUsersDbSetWithExistingUsers(IEnumerable<Nyss.Data.Models.User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);


            foreach (var user in existingUsers)
            {
                _nyssContext.Users.FindAsync(user.Id).Returns(user);
            }
        }

        private void ArrangeUSersDbSetWithOneGlobalCoordinator() =>
            ArrangeUsersDbSetWithExistingUsers(new List<Nyss.Data.Models.User>
            {
                new GlobalCoordinatorUser
                {
                    Id = 123,
                    Role = Role.GlobalCoordinator,
                    EmailAddress = "emailTest1@domain.com",
                    Name = "emailTest1@domain.com",
                    Organization = "org org",
                    PhoneNumber = "123"
                }
            });

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var registerGlobalCoordinatorRequestDto = new CreateGlobalCoordinatorRequestDto
            {
                Name = userName,
                Email = userEmail
            };

            var result = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<Nyss.Data.Models.User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new CreateGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            var result = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<GlobalCoordinatorUser>());
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new CreateGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            var result = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterGlobalCoordinator_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new CreateGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            var result = await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void RegisterGlobalCoordinator_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerGlobalCoordinatorRequestDto = new CreateGlobalCoordinatorRequestDto { Name = userEmail, Email = userEmail };


            _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto).ShouldThrowAsync<Exception>();
        }

        [Fact]
        public async Task EditGlobalCoordinator_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            var nyssUsers = new List<Nyss.Data.Models.User>();
            var usersDbSet = nyssUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);


            var result = await _globalCoordinatorService.UpdateGlobalCoordinator(new EditGlobalCoordinatorRequestDto() {Id = 123});


            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditGlobalCoordinator_WhenEditingUserThatIsNotGlobalCoordinator_ReturnsErrorResult()
        {
            ArrangeUsersDbSetWithOneAdministratorUser();

            var result = await _globalCoordinatorService.UpdateGlobalCoordinator(new EditGlobalCoordinatorRequestDto() { Id = 123 });

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditGlobalCoordinator_WhenEditingUserThatIsNotGlobalCoordinator_SaveChangesShouldNotBeCalled()
        {
           ArrangeUsersDbSetWithOneAdministratorUser();

            await _globalCoordinatorService.UpdateGlobalCoordinator(new EditGlobalCoordinatorRequestDto() { Id = 123 });

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task EditGlobalCoordinator_WhenEditingExistingGlobalCoordinator_ReturnsSuccess()
        {
            ArrangeUSersDbSetWithOneGlobalCoordinator();

            var result = await _globalCoordinatorService.UpdateGlobalCoordinator(new EditGlobalCoordinatorRequestDto() { Id = 123 });

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EditGlobalCoordinator_WhenEditingExistingGlobalCoordinator_SaveChangesAsyncIsCalled()
        {
            ArrangeUSersDbSetWithOneGlobalCoordinator();

            await _globalCoordinatorService.UpdateGlobalCoordinator(new EditGlobalCoordinatorRequestDto() { Id = 123 });

            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task EditGlobalCoordinator_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUSersDbSetWithOneGlobalCoordinator();

            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var editRequest = new EditGlobalCoordinatorRequestDto()
            {
                Id = 123, Name = "New name", Organization = "New organization", PhoneNumber = "432432"
            };

            await _globalCoordinatorService.UpdateGlobalCoordinator(editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as GlobalCoordinatorUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
        }

        [Fact]
        public async Task GetGlobalCoordinators_OnlyGlobalCoordinatorsAreReturned()
        {
            var nyssUsers = new List<Nyss.Data.Models.User>()
            {
                new GlobalCoordinatorUser() { Id = 1, Role = Role.GlobalCoordinator },
                new GlobalCoordinatorUser() { Id = 2, Role = Role.GlobalCoordinator },
                new GlobalCoordinatorUser() { Id = 3, Role = Role.Administrator },
                new GlobalCoordinatorUser() { Id = 4, Role = Role.Supervisor},
                new GlobalCoordinatorUser() { Id = 5, Role = Role.TechnicalAdvisor },
                new GlobalCoordinatorUser() { Id = 6, Role = Role.DataConsumer },
                new GlobalCoordinatorUser() { Id = 7, Role = Role.Supervisor },
                new GlobalCoordinatorUser() { Id = 8, Role = Role.Supervisor },
                new GlobalCoordinatorUser() { Id = 9, Role = Role.Supervisor }
            };
            ArrangeUsersDbSetWithExistingUsers(nyssUsers);

            var result = await _globalCoordinatorService.GetGlobalCoordinators();

            result.Value.Count.ShouldBe(2);
        }

        [Fact]
        public async Task RemoveGlobalCoordinator_WhenDeleting_EnsureCanDeleteUserIsCalled()
        {
            //arrange
            ArrangeUSersDbSetWithOneGlobalCoordinator();

            //act
            await _globalCoordinatorService.RemoveGlobalCoordinator(123);

            //assert
            await _deleteUserService.Received().EnsureCanDeleteUser(123, Role.GlobalCoordinator);
        }
    }
}
