using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Agreements;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Agreements
{
    public class AgreementServiceTests
    {
        private readonly IAgreementService _agreementService;
        private readonly INyssContext _nyssContextMock;
        private readonly IGeneralBlobProvider _generalBlobProviderMock;
        private readonly IDataBlobService _dataBlobServiceMock;
        private readonly IAuthorizationService _authorizationServiceMock;
        private readonly IDateTimeProvider _dateTimeProviderMock;


        public AgreementServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _authorizationServiceMock = Substitute.For<IAuthorizationService>();
            _generalBlobProviderMock = Substitute.For<IGeneralBlobProvider>();
            _dataBlobServiceMock = Substitute.For<IDataBlobService>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

            // Current user stuff
            _authorizationServiceMock.GetCurrentUserName().Returns("yo");
            _authorizationServiceMock.IsCurrentUserInAnyRole(Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator).Returns(true);

            _agreementService = new AgreementService(_authorizationServiceMock, _nyssContextMock, _generalBlobProviderMock, _dataBlobServiceMock, _dateTimeProviderMock);
        }

        [Fact]
        public async Task AcceptAgreement_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var users = new List<User> { new ManagerUser { EmailAddress = "no-yo" } };
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(mockDbSet);

            // Act
            var result = await _agreementService.AcceptAgreement("fr");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }

        [Fact]
        public async Task GetPendingAgreements_WhenNewPending_ShouldIncludePending()
        {
            // Arrange
            _authorizationServiceMock.IsCurrentUserInRole(Role.Coordinator).Returns(true);
            var users = new List<User>
            {
                new ManagerUser
                {
                    EmailAddress = "yo",
                    Id = 1
                }
            };
            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(usersDbSet);

            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Name = "Narnia",
                    NationalSocietyUsers = new List<UserNationalSociety> { new UserNationalSociety { UserId = 1 } }
                }
            };
            var nsDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nsDbSet);

            var consentsDbSet = new List<NationalSocietyConsent>().AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocietyConsents.Returns(consentsDbSet);


            // Act
            var result = await _agreementService.GetPendingAgreements();

            // Assert
            result.Value.PendingSocieties.Count().ShouldBe(1);
            result.Value.PendingSocieties.FirstOrDefault().ShouldBe("Narnia");
        }

        [Fact]
        public async Task GetPendingAgreements_WhenUpdatedDoc_ShouldIncludeStale()
        {
            // Arrange
            var consentedDate = new DateTime(2020, 10, 10, 13, 37, 00);
            _generalBlobProviderMock.GetPlatformAgreementLastModifiedDate("klingon").Returns(consentedDate.AddDays(1)); // one day newer
            _authorizationServiceMock.IsCurrentUserInRole(Role.Coordinator).Returns(true);
            var users = new List<User>
            {
                new ManagerUser
                {
                    EmailAddress = "yo",
                    Id = 1,
                    ApplicationLanguage = new ApplicationLanguage { LanguageCode = "klingon" }
                }
            };
            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(usersDbSet);

            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Name = "Narnia",
                    Id = 1,
                    NationalSocietyUsers = new List<UserNationalSociety> { new UserNationalSociety { UserId = 1 } }
                }
            };
            var nsDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nsDbSet);

            var consents = new List<NationalSocietyConsent>
            {
                new NationalSocietyConsent
                {
                    UserEmailAddress = "yo",
                    ConsentedFrom = consentedDate,
                    NationalSocietyId = 1
                }
            };
            var mockDbSet = consents.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocietyConsents.Returns(mockDbSet);

            // Act
            var result = await _agreementService.GetPendingAgreements();

            // Assert
            await _generalBlobProviderMock.Received(1).GetPlatformAgreementLastModifiedDate("klingon");
            result.Value.StaleSocieties.Count().ShouldBe(1);
        }
    }
}
