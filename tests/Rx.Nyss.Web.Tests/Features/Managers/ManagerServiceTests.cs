using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.Managers.Dto;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Managers
{
    public class ManagerServiceTests
    {
        private readonly ManagerService _managerService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;

        private readonly int _administratorId = 1;
        private readonly int _managerId = 2;
        private readonly int _nationalSocietyId = 1;
        private readonly int _organizationId = 1;
        private readonly IDeleteUserService _deleteUserService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationService _organizationService;


        public ManagerServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();
            _deleteUserService = Substitute.For<IDeleteUserService>();

            _authorizationService = Substitute.For<IAuthorizationService>();
            _organizationService = Substitute.For<IOrganizationService>();
            _managerService = new ManagerService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock, _deleteUserService,
                _authorizationService, _organizationService);

            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = _nationalSocietyId,
                    Name = "Test national society"
                }
            };
            var applicationLanguages = new List<ApplicationLanguage>();

            var administratorUser = new AdministratorUser
            {
                Id = _administratorId,
                Role = Role.Administrator
            };

            var managerUser = new ManagerUser
            {
                Id = _managerId,
                Role = Role.Manager,
                EmailAddress = "emailTest1@domain.com",
                Name = "emailTest1@domain.com",
                Organization = "org org",
                PhoneNumber = "123",
                AdditionalPhoneNumber = "321"
            };

            var users = new List<User>
            {
                administratorUser,
                managerUser
            };

            var organizations = new List<Organization>
            {
                new Organization
                {
                    Id = _organizationId,
                    Name = "test org 1"
                }
            };

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    User = users[1],
                    UserId = _managerId,
                    NationalSocietyId = _nationalSocietyId,
                    NationalSociety = nationalSocieties[0],
                    OrganizationId = _organizationId,
                    Organization = organizations[0]
                },
                new UserNationalSociety
                {
                    User = users[0],
                    UserId = _administratorId,
                    NationalSocietyId = _nationalSocietyId,
                    NationalSociety = nationalSocieties[0],
                    OrganizationId = _organizationId,
                    Organization = organizations[0]
                }
            };
            users[1].UserNationalSocieties = new List<UserNationalSociety> { userNationalSocieties[0] };

            _authorizationService.GetCurrentUser().Returns(managerUser);
            _authorizationService.GetCurrentUser().Returns(managerUser);

            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            var organizationsDbSet = organizations.AsQueryable().BuildMockDbSet();

            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);
            _nyssContext.Users.Returns(usersDbSet);
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
            _nyssContext.Organizations.Returns(organizationsDbSet);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser
            {
                Id = "123",
                Email = (string)ci[0]
            });
            nationalSocieties[0].NationalSocietyUsers = userNationalSocieties;
            _nyssContext.NationalSocieties.FindAsync(_nationalSocietyId).Returns(nationalSocieties[0]);
            _nyssContext.Organizations.FindAsync(_organizationId).Returns(organizations[0]);

            _nationalSocietyUserService.GetNationalSocietyUser<ManagerUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = users.OfType<ManagerUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }

                return user;
            });

            _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<ManagerUser>(Arg.Any<int>())
                .Returns(ci => _nationalSocietyUserService.GetNationalSocietyUser<ManagerUser>((int)ci[0]));
            _nationalSocietyUserService.GetNationalSocietyUserIncludingOrganizations<ManagerUser>(Arg.Any<int>())
                .Returns(ci => _nationalSocietyUserService.GetNationalSocietyUser<ManagerUser>((int)ci[0]));
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

            var result = await _managerService.Create(_nationalSocietyId, registerManagerRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto
            {
                Name = userEmail,
                Email = userEmail
            };

            var result = await _managerService.Create(_nationalSocietyId, registerManagerRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto
            {
                Name = userEmail,
                Email = userEmail
            };

            var result = await _managerService.Create(_nationalSocietyId, registerManagerRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterManager_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto
            {
                Name = userEmail,
                Email = userEmail
            };

            var nationalSocietyId = 1;
            var result = await _managerService.Create(nationalSocietyId, registerManagerRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void RegisterManager_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerManagerRequestDto = new CreateManagerRequestDto
            {
                Name = userEmail,
                Email = userEmail
            };

            _managerService.Create(_nationalSocietyId, registerManagerRequestDto).ShouldThrowAsync<Exception>();
        }


        [Fact]
        public async Task EditManager_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            var result = await _managerService.Edit(999, new EditManagerRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditManager_WhenEditingUserThatIsNotManager_ReturnsErrorResult()
        {
            var result = await _managerService.Edit(_administratorId, new EditManagerRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }


        [Fact]
        public async Task EditManager_WhenEditingUserThatIsNotManager_SaveChangesShouldNotBeCalled()
        {
            await _managerService.Edit(_administratorId, new EditManagerRequestDto());

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task EditManager_WhenEditingExistingManager_ReturnsSuccess()
        {
            var result = await _managerService.Edit(_managerId, new EditManagerRequestDto());

            result.IsSuccess.ShouldBeTrue();
        }


        [Fact]
        public async Task EditManager_WhenEditingExistingManager_SaveChangesAsyncIsCalled()
        {
            await _managerService.Edit(2, new EditManagerRequestDto());

            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task EditManager_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == _managerId)?.EmailAddress;

            var editRequest = new EditManagerRequestDto
            {
                Name = "New name",
                Organization = "New organization",
                PhoneNumber = "456",
                AdditionalPhoneNumber = "654"
            };

            await _managerService.Edit(_managerId, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == _managerId) as ManagerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.AdditionalPhoneNumber.ShouldBe(editRequest.AdditionalPhoneNumber);
        }

        [Fact]
        public async Task DeleteManager_WhenDeletingAPendingHeadManager_NationalSocietyPendingManagerGetsNullified()
        {
            //arrange
            var manager = _nyssContext.Users.Single(x => x.Id == _managerId);
            var organization = _nyssContext.Organizations.Single(x => x.Id == _organizationId);
            organization.PendingHeadManager = manager;

            //act
            await _managerService.Delete(_managerId);

            //assert
            organization.PendingHeadManager.ShouldBe(null);
        }
    }
}
