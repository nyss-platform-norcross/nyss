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
using RX.Nyss.Web.Features.DataManager;
using RX.Nyss.Web.Features.DataManager.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.DataManager
{
    public class DataManagerServiceTests
    {
        private readonly DataManagerService _dataManagerService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;

        public DataManagerServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _dataManagerService = new DataManagerService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock);

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
        public async Task RegisterDataManager_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var registerDataManagerRequestDto = new CreateDataManagerRequestDto
            {
                Name = userName,
                Email = userEmail
            };

            var nationalSocietyId = 1;
            var result = await _dataManagerService.CreateDataManager(nationalSocietyId, registerDataManagerRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RegisterDataManager_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerDataManagerRequestDto = new CreateDataManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _dataManagerService.CreateDataManager(nationalSocietyId, registerDataManagerRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task RegisterDataManager_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerDataManagerRequestDto = new CreateDataManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _dataManagerService.CreateDataManager(nationalSocietyId, registerDataManagerRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterDataManager_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerDataManagerRequestDto = new CreateDataManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _dataManagerService.CreateDataManager(nationalSocietyId, registerDataManagerRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void RegisterDataManager_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerDataManagerRequestDto = new CreateDataManagerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            _dataManagerService.CreateDataManager(nationalSocietyId, registerDataManagerRequestDto).ShouldThrowAsync<Exception>();
        }


        [Fact]
        public async Task EditDataManager_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            ArrangeUsersFrom(new List<User> { });


            var result = await _dataManagerService.UpdateDataManager(123, new EditDataManagerRequestDto() {});


            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditDataManager_WhenEditingUserThatIsNotDataManager_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var result = await _dataManagerService.UpdateDataManager(123, new EditDataManagerRequestDto() { });

            result.IsSuccess.ShouldBeFalse();
        }

        private void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User> { new AdministratorUser() { Id = 123, Role = Role.Administrator } });

        private void ArrangeUsersFrom(IEnumerable<User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);

            _nationalSocietyUserService.GetNationalSocietyUser<DataManagerUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = existingUsers.OfType<DataManagerUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                return user;
            });
        }
        
        [Fact]
        public async Task EditDataManager_WhenEditingUserThatIsNotDataManager_SaveChangesShouldNotBeCalled()
        {
            ArrangeUsersWithOneAdministratorUser();

            await _dataManagerService.UpdateDataManager(123, new EditDataManagerRequestDto() { });

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task EditDataManager_WhenEditingExistingDataManager_ReturnsSuccess()
        {
            ArrangeUSersDbSetWithOneDataManager();

            var result = await _dataManagerService.UpdateDataManager(123, new EditDataManagerRequestDto() {  });

            result.IsSuccess.ShouldBeTrue();
        }

        private void ArrangeUSersDbSetWithOneDataManager() =>
            ArrangeUsersFrom(new List<User>
            {
                new DataManagerUser
                {
                    Id = 123,
                    Role = Role.DataManager,
                    EmailAddress = "emailTest1@domain.com",
                    Name = "emailTest1@domain.com",
                    Organization = "org org",
                    PhoneNumber = "123"
                }
            });


        [Fact]
        public async Task EditDataManager_WhenEditingExistingDataManager_SaveChangesAsyncIsCalled()
        {
            ArrangeUSersDbSetWithOneDataManager();

            await _dataManagerService.UpdateDataManager(123, new EditDataManagerRequestDto() {  });

            await _nyssContext.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task EditDataManager_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUSersDbSetWithOneDataManager();

            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var editRequest = new EditDataManagerRequestDto()
            {
                Name = "New name",
                Organization = "New organization",
                PhoneNumber = "432432"
            };

            await _dataManagerService.UpdateDataManager(123, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as DataManagerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
        }

        
    }
}
