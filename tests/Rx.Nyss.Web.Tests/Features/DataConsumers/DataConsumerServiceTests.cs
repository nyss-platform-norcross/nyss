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
using RX.Nyss.Web.Features.DataConsumers;
using RX.Nyss.Web.Features.DataConsumers.Dto;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataConsumers
{
    public class DataConsumerServiceTests
    {
        private readonly DataConsumerService _dataConsumerService;

        private readonly ILoggerAdapter _loggerAdapter;

        private readonly INyssContext _nyssContext;

        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;

        private readonly INationalSocietyUserService _nationalSocietyUserService;

        private readonly IVerificationEmailService _verificationEmailServiceMock;

        public DataConsumerServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();

            _dataConsumerService = new DataConsumerService(
                _identityUserRegistrationServiceMock,
                _nationalSocietyUserService,
                _nyssContext,
                _loggerAdapter,
                _verificationEmailServiceMock);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser
            {
                Id = "123",
                Email = (string)ci[0]
            });

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

        private void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User>
            {
                new AdministratorUser
                {
                    Id = 123,
                    Role = Role.Administrator
                }
            });


        private void SetupTestNationalSocieties()
        {
            var nationalSociety1 = new NationalSociety
            {
                Id = 1,
                Name = "Test national society 1",
                Organizations = new List<Organization>
                {
                    new Organization { Name = "Org 1 in NS 1" },
                    new Organization { Name = "Org 2 in NS 1" }
                }
            };
            var nationalSociety2 = new NationalSociety
            {
                Id = 2,
                Name = "Test national society 2",
                Organizations = new List<Organization>
                {
                    new Organization { Name = "Org 1 in NS 2" },
                    new Organization { Name = "Org 2 in NS 2" }
                }
            };
            var nationalSocieties = new List<NationalSociety>
            {
                nationalSociety1,
                nationalSociety2
            };
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSociety1);
            _nyssContext.NationalSocieties.FindAsync(2).Returns(nationalSociety2);
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            ArrangeUsersFrom(new List<User>());


            var result = await _dataConsumerService.Edit(123, new EditDataConsumerRequestDto());


            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingUserThatIsNotDataConsumer_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var result = await _dataConsumerService.Edit(123, new EditDataConsumerRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingUserThatIsNotDataConsumer_SaveChangesShouldNotBeCalled()
        {
            ArrangeUsersWithOneAdministratorUser();

            await _dataConsumerService.Edit(123, new EditDataConsumerRequestDto());

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task EditDataConsumer_WhenEditingExistingDataConsumer_ReturnsSuccess()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            var result = await _dataConsumerService.Edit(123, new EditDataConsumerRequestDto());

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EditDataConsumer_WhenEditingExistingDataConsumer_SaveChangesAsyncIsCalled()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            await _dataConsumerService.Edit(123, new EditDataConsumerRequestDto());

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

            await _dataConsumerService.Edit(123, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as DataConsumerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.Organization.ShouldBe(editRequest.Organization);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.AdditionalPhoneNumber.ShouldBe(editRequest.AdditionalPhoneNumber);
        }
    }
}
