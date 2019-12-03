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
using RX.Nyss.Web.Features.Manager;
using RX.Nyss.Web.Features.Manager.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.Manager
{
    public class ManagerServiceTests
    {
        private readonly ManagerService _managerService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;

        public ManagerServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _managerService = new ManagerService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });

            SetupTestNationalSociety();
        }

        private void SetupTestNationalSociety()
        {
            var nationalSociety1 = new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "Test national society"};
            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety> { nationalSociety1 }; 
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSociety1);
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var registerManagerRequestDto = new CreateManagerRequestDto
            {
                Name = userName,
                Email = userEmail
            };

            var nationalSocietyId = 1;
            var result = await _managerService.CreateManager(nationalSocietyId, registerManagerRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _managerService.CreateManager(nationalSocietyId, registerManagerRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _managerService.CreateManager(nationalSocietyId, registerManagerRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _managerService.CreateManager(nationalSocietyId, registerManagerRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void RegisterManager_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            _managerService.CreateManager(nationalSocietyId, registerManagerRequestDto).ShouldThrowAsync<Exception>();
        }


        [Fact]
        public async Task EditManager_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            ArrangeUsersFrom(new List<User> { });


            var result = await _managerService.UpdateManager(123, new EditManagerRequestDto() {});


            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditManager_WhenEditingUserThatIsNotManager_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var result = await _managerService.UpdateManager(123, new EditManagerRequestDto() { });

            result.IsSuccess.ShouldBeFalse();
        }

        private void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User> { new AdministratorUser() { Id = 123, Role = Role.Administrator } });

        private void ArrangeUsersFrom(IEnumerable<User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);

            _nationalSocietyUserService.GetNationalSocietyUser<ManagerUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = existingUsers.OfType<ManagerUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                return user;
            });
        }
        
        [Fact]
        public async Task EditManager_WhenEditingUserThatIsNotManager_SaveChangesShouldNotBeCalled()
        {
            ArrangeUsersWithOneAdministratorUser();

            await _managerService.UpdateManager(123, new EditManagerRequestDto() { });

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task EditManager_WhenEditingExistingManager_ReturnsSuccess()
        {
            ArrangeUSersDbSetWithOneManager();

            var result = await _managerService.UpdateManager(123, new EditManagerRequestDto() {  });

            result.IsSuccess.ShouldBeTrue();
        }

        private void ArrangeUSersDbSetWithOneManager() =>
            ArrangeUsersFrom(new List<User>
            {
                new ManagerUser
                {
                    Id = 123,
                    Role = Role.Manager,
                    EmailAddress = "emailTest1@domain.com",
                    Name = "emailTest1@domain.com",
                    Organization = "org org",
                    PhoneNumber = "123",
                    AdditionalPhoneNumber = "321"
                }
            });

        [Fact]
        public async Task EditManager_WhenEditingExistingManager_SaveChangesAsyncIsCalled()
        {
            ArrangeUSersDbSetWithOneManager();

            await _managerService.UpdateManager(123, new EditManagerRequestDto() {  });

            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task EditManager_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUSersDbSetWithOneManager();

            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var editRequest = new EditManagerRequestDto()
            {
                Name = "New name",
                Organization = "New organization",
                PhoneNumber = "456",
                AdditionalPhoneNumber = "654"
            };

            await _managerService.UpdateManager(123, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as ManagerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.AdditionalPhoneNumber.ShouldBe(editRequest.AdditionalPhoneNumber);
        }
    }
}
