using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.TechnicalAdvisors;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Tests.Features.Organizations.TestData;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Organizations
{
    public class OrganizationServiceTests
    {
        private readonly IOrganizationService _nationalSocietyService;
        private readonly OrganizationServiceTestData _testData;

        private readonly INyssContext _nyssContextMock;
        private readonly ISmsGatewayService _smsGatewayServiceMock;

        private readonly IManagerService _managerServiceMock;
        private readonly ITechnicalAdvisorService _technicalAdvisorServiceMock;
        private readonly IGeneralBlobProvider _generalBlobProviderMock;
        private readonly IDataBlobService _dataBlobServiceMock;


        public OrganizationServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            var authorizationService = Substitute.For<IAuthorizationService>();
            authorizationService.GetCurrentUserName().Returns("yo");
            _managerServiceMock = Substitute.For<IManagerService>();
            _technicalAdvisorServiceMock = Substitute.For<ITechnicalAdvisorService>();
            _smsGatewayServiceMock = Substitute.For<ISmsGatewayService>();
            _generalBlobProviderMock = Substitute.For<IGeneralBlobProvider>();
            _dataBlobServiceMock = Substitute.For<IDataBlobService>();

            _nationalSocietyService = new OrganizationService(_nyssContextMock, loggerAdapterMock, authorizationService, _generalBlobProviderMock, _dataBlobServiceMock);

            _testData = new OrganizationServiceTestData(_nyssContextMock, _smsGatewayServiceMock);
        }

        [Theory]
        [InlineData(Role.Coordinator)]
        [InlineData(Role.Manager)]
        public async Task SetAsHead_WhenOk_ShouldBeOk(Role role)
        {
            // Arrange
            var sourceUri = "https://yo.example.com";
            _generalBlobProviderMock.GetPlatformAgreementUrl("en").Returns(sourceUri);
            _testData.BasicData.WhenNoConsentsAndRole(role).GenerateData().AddToDbContext();

            // Act
            var result = await _nationalSocietyService.SetAsHeadManager("en");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            await _dataBlobServiceMock.Received(1).StorePlatformAgreement(sourceUri, Arg.Any<string>());
            await _nyssContextMock.NationalSocietyConsents.Received(1).AddAsync(Arg.Any<NationalSocietyConsent>());
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SetAsHead_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var users = new List<User> { new ManagerUser { EmailAddress = "no-yo" } };
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(mockDbSet);

            // Act
            var result = await _nationalSocietyService.SetAsHeadManager("fr");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }
    }
}
