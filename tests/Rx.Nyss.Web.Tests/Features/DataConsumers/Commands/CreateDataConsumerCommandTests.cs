using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataConsumers.Commands;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataConsumers.Commands
{
    public class CreateDataConsumerCommandTests
    {
        private readonly INyssContext _mockDataContext;

        private readonly IIdentityUserRegistrationService _mockIdentityUserRegistrationService;

        private readonly IVerificationEmailService _mockVerificationEmailService;

        private readonly ILoggerAdapter _mockLoggerAdapter;

        private readonly CreateDataConsumerCommand.Handler _handler;

        public CreateDataConsumerCommandTests()
        {
            _mockDataContext = Substitute.For<INyssContext>();
            _mockIdentityUserRegistrationService = Substitute.For<IIdentityUserRegistrationService>();
            _mockVerificationEmailService = Substitute.For<IVerificationEmailService>();
            _mockLoggerAdapter = Substitute.For<ILoggerAdapter>();

            _handler = new CreateDataConsumerCommand.Handler(
                _mockDataContext,
                _mockIdentityUserRegistrationService,
                _mockVerificationEmailService,
                _mockLoggerAdapter);

            SetupTestNationalSocieties();
        }

        [Fact]
        public async Task WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            // Arrange
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var cmd = new CreateDataConsumerCommand
            {
                NationalSocietyId = 1,
                Name = userName,
                Email = userEmail
            };

            _mockIdentityUserRegistrationService.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser
            {
                Id = "123",
                Email = (string)ci[0]
            });

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await _mockIdentityUserRegistrationService.Received(1).GenerateEmailVerification(userEmail);
            await _mockVerificationEmailService.Received(1).SendVerificationForDataConsumersEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), "Org 1 in NS 1, Org 2 in NS 1", Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            // Arrange
            var userEmail = "emailTest1@domain.com";
            var cmd = new CreateDataConsumerCommand
            {
                NationalSocietyId = 1,
                Name = userEmail,
                Email = userEmail
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await _mockDataContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            // Arrange
            var userEmail = "emailTest1@domain.com";
            var cmd = new CreateDataConsumerCommand
            {
                NationalSocietyId = 1,
                Name = userEmail,
                Email = userEmail
            };

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await _mockDataContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            // Arrange
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            var userEmail = "emailTest1@domain.com";
            var cmd = new CreateDataConsumerCommand
            {
                NationalSocietyId = 1,
                Name = userEmail,
                Email = userEmail
            };

            _mockIdentityUserRegistrationService.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            // Arrange
            var userEmail = "emailTest1@domain.com";
            var cmd = new CreateDataConsumerCommand
            {
                Name = userEmail,
                Email = userEmail
            };

            _mockIdentityUserRegistrationService.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            // Act & Assert
            _handler.Handle(cmd, CancellationToken.None).ShouldThrowAsync<Exception>();
        }

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
            _mockDataContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _mockDataContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _mockDataContext.NationalSocieties.FindAsync(1).Returns(nationalSociety1);
            _mockDataContext.NationalSocieties.FindAsync(2).Returns(nationalSociety2);
        }
    }
}
