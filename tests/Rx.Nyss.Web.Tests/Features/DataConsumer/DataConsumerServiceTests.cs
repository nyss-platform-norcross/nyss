using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataConsumer;
using RX.Nyss.Web.Features.DataConsumer.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataConsumer
{
    public class DataConsumerServiceTests
    {
        private readonly DataConsumerService _dataConsumerService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;
        private readonly IDeleteUserService _deleteUserService;

        public DataConsumerServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();
            _deleteUserService = Substitute.For<IDeleteUserService>();

            _dataConsumerService = new DataConsumerService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock, _deleteUserService);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });

            SetupTestNationalSocieties();
        }

        private User ArrangeUsersDbSetWithOneDataConsumer()
        {
            var dataConsumer = new DataConsumerUser
            {
                Id = 123,
                Role = Role.DataConsumer,
                EmailAddress = "emailTest1@domain.com",
                Name = "emailTest1@domain.com",
                Organization = "org org",
                PhoneNumber = "123",
                AdditionalPhoneNumber = "321"
            };
            ArrangeUsersFrom(new List<User> { dataConsumer });
            return dataConsumer;
        }

        private User ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety()
        {
            var dataConsumer = ArrangeUsersDbSetWithOneDataConsumer();

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety {User = dataConsumer, UserId = dataConsumer.Id, NationalSocietyId = 1, NationalSociety = _nyssContext.NationalSocieties.Find(1)}
            };

            ArrangeUserNationalSocietiesFrom(userNationalSocieties);
            dataConsumer.UserNationalSocieties = userNationalSocieties;

            return dataConsumer;
        }


        private User ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties()
        {
            var dataConsumer = ArrangeUsersDbSetWithOneDataConsumer();

            var userNationalSocieties= new List<UserNationalSociety>
            {
                new UserNationalSociety {User = dataConsumer, UserId = dataConsumer.Id, NationalSocietyId = 1, NationalSociety = _nyssContext.NationalSocieties.Find(1)},
                new UserNationalSociety {User = dataConsumer, UserId = dataConsumer.Id, NationalSocietyId = 2, NationalSociety = _nyssContext.NationalSocieties.Find(2)}
            };

            ArrangeUserNationalSocietiesFrom(userNationalSocieties);
            dataConsumer.UserNationalSocieties = userNationalSocieties;

            return dataConsumer;
        }

        private void ArrangeUsersFrom(IEnumerable<User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);

            _nationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = existingUsers.OfType<DataConsumerUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                return user;
            });

            _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<DataConsumerUser>(Arg.Any<int>())
                .Returns(ci => _nationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>((int)ci[0]));
        }

        private void ArrangeUserNationalSocietiesFrom(IEnumerable<UserNationalSociety> userNationalSocieties)
        {
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
        }

        private void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User> { new AdministratorUser { Id = 123, Role = Role.Administrator } });


        private void SetupTestNationalSocieties()
        {
            var nationalSociety1 = new NationalSociety {Id = 1, Name = "Test national society 1"};
            var nationalSociety2 = new NationalSociety { Id = 2, Name = "Test national society 2" };
            var nationalSocieties = new List<NationalSociety> { nationalSociety1 , nationalSociety2 };
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSociety1);
            _nyssContext.NationalSocieties.FindAsync(2).Returns(nationalSociety2);
        }

        [Fact]
        public async Task RegisterDataConsumer_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var registerDataConsumerRequestDto = new CreateDataConsumerRequestDto
            {
                Name = userName,
                Email = userEmail
            };

            var nationalSocietyId = 1;
            var result = await _dataConsumerService.CreateDataConsumer(nationalSocietyId, registerDataConsumerRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RegisterDataConsumer_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerDataConsumerRequestDto = new CreateDataConsumerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _dataConsumerService.CreateDataConsumer(nationalSocietyId, registerDataConsumerRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task RegisterDataConsumer_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerDataConsumerRequestDto = new CreateDataConsumerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _dataConsumerService.CreateDataConsumer(nationalSocietyId, registerDataConsumerRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterDataConsumer_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerDataConsumerRequestDto = new CreateDataConsumerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _dataConsumerService.CreateDataConsumer(nationalSocietyId, registerDataConsumerRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void RegisterDataConsumer_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerDataConsumerRequestDto = new CreateDataConsumerRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            _dataConsumerService.CreateDataConsumer(nationalSocietyId, registerDataConsumerRequestDto).ShouldThrowAsync<Exception>();
        }


        [Fact]
        public async Task EditDataConsumer_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            ArrangeUsersFrom(new List<User>());


            var result = await _dataConsumerService.UpdateDataConsumer(123, new EditDataConsumerRequestDto());


            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingUserThatIsNotDataConsumer_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var result = await _dataConsumerService.UpdateDataConsumer(123, new EditDataConsumerRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingUserThatIsNotDataConsumer_SaveChangesShouldNotBeCalled()
        {
            ArrangeUsersWithOneAdministratorUser();

            await _dataConsumerService.UpdateDataConsumer(123, new EditDataConsumerRequestDto());

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task EditDataConsumer_WhenEditingExistingDataConsumer_ReturnsSuccess()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            var result = await _dataConsumerService.UpdateDataConsumer(123, new EditDataConsumerRequestDto());

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingExistingDataConsumer_SaveChangesAsyncIsCalled()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            await _dataConsumerService.UpdateDataConsumer(123, new EditDataConsumerRequestDto());

            await _nyssContext.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task EditDataConsumer_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var editRequest = new EditDataConsumerRequestDto
            {
                Name = "New name",
                Organization = "New organization",
                PhoneNumber = "456",
                AdditionalPhoneNumber = "654"
            };

            await _dataConsumerService.UpdateDataConsumer(123, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as DataConsumerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.AdditionalPhoneNumber.ShouldBe(editRequest.AdditionalPhoneNumber);
        }

        [Fact]
        public async Task DeleteDataConsumer_WhenSuccess_SaveChangesIsCalledOnce()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();

            //act
            await _dataConsumerService.DeleteDataConsumer(1, 123);

            //assert
            await _nyssContext.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteDataConsumer_WhenDeletingFromNationalSocietyTheUserIsNotIn_ShouldReturnError()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();

            //act
            var result = await _dataConsumerService.DeleteDataConsumer(2, 123);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserIsNotAssignedToThisNationalSociety);
        }

        [Fact]
        public async Task DeleteDataConsumer_WhenDeletingAUserThatDoesNotExist_ShouldReturnError()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();

            //act
            var result = await _dataConsumerService.DeleteDataConsumer(2, 321);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserNotFound);
        }


        [Fact]
        public async Task DeleteDataConsumer_WhenDeletingFromLastNationalSociety_RemoveOnNyssUserIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();

            //act
            await _dataConsumerService.DeleteDataConsumer(1, 123);

            //assert
            _nationalSocietyUserService.Received(1).DeleteNationalSocietyUser(user);
        }

        [Fact]
        public async Task DeleteDataConsumer_WhenDeletingFromLastNationalSociety_RemoveOnNationalSocietyReferenceIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();

            //act
            await _dataConsumerService.DeleteDataConsumer(1, 123);

            //assert
            _nyssContext.UserNationalSocieties.Received(1).Remove(Arg.Is<UserNationalSociety>(uns => uns.NationalSocietyId == 1 && uns.UserId == 123));
        }

        [Fact]
        public async Task DeleteDataConsumer_WhenDeletingFromNotLastNationalSociety_RemoveOnNyssUserIsNotCalled()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties();

            //act
            await _dataConsumerService.DeleteDataConsumer(1, 123);

            //assert
            _nationalSocietyUserService.Received(0).DeleteNationalSocietyUser(user);
        }

        [Fact]
        public async Task DeleteDataConsumer_WhenDeletingFromNotLastNationalSociety_RemoveOnNationalSocietyReferenceIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties();

            //act
            await _dataConsumerService.DeleteDataConsumer(1, 123);

            //assert
            _nyssContext.UserNationalSocieties.Received(1).Remove(Arg.Is<UserNationalSociety>(uns => uns.NationalSocietyId == 1 && uns.UserId == 123));
        }

        [Fact]
        public async Task DeleteTechnicalAdvisor_WhenDeleting_EnsureCanDeleteUserIsCalled()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties();

            //act
            await _dataConsumerService.DeleteDataConsumer(1, 123);

            //assert
            await _deleteUserService.Received().EnsureCanDeleteUser(123, Role.DataConsumer);
        }
    }
}
