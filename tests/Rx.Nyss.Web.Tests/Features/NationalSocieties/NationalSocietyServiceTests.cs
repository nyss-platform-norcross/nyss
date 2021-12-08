using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocieties;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Tests.Features.NationalSocieties.TestData;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties
{
    public class NationalSocietyServiceTests
    {
        private readonly INationalSocietyService _nationalSocietyService;

        private readonly NationalSocietyServiceTestData _testData;

        private readonly INyssContext _nyssContextMock;

        private readonly ISmsGatewayService _smsGatewayServiceMock;

        private readonly IAuthorizationService _authorizationServiceMock;

        public NationalSocietyServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _authorizationServiceMock = Substitute.For<IAuthorizationService>();
            _authorizationServiceMock.GetCurrentUserName().Returns("yo");
            _smsGatewayServiceMock = Substitute.For<ISmsGatewayService>();

            _nationalSocietyService = new NationalSocietyService(
                _nyssContextMock,
                Substitute.For<INationalSocietyAccessService>(),
                _authorizationServiceMock);

            _testData = new NationalSocietyServiceTestData(_nyssContextMock, _smsGatewayServiceMock);
        }

        [Fact]
        public async Task ReopenNationalSociety_WhenSucess_ArchivedFlagIsFalse()
        {
            //arrange
            _testData.WhenReopeningNationalSociety.GenerateData().AddToDbContext();
            var nationalSocietyBeingRestored = _testData.WhenReopeningNationalSociety.AdditionalData.NationalSocietyBeingReopened;

            //act
            var result = await _nationalSocietyService.Reopen(nationalSocietyBeingRestored.Id);

            //assert
            result.IsSuccess.ShouldBeTrue();
            nationalSocietyBeingRestored.IsArchived.ShouldBeFalse();
        }

        [Fact]
        public async Task ReopenNationalSociety_WhenSuccess_SaveChangesIsCalled()
        {
            //arrange
            _testData.WhenReopeningNationalSociety.GenerateData().AddToDbContext();
            var nationalSocietyBeingRestoredId = _testData.WhenReopeningNationalSociety.AdditionalData.NationalSocietyBeingReopened.Id;

            //act
            var result = await _nationalSocietyService.Reopen(nationalSocietyBeingRestoredId);

            //assert
            result.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received().SaveChangesAsync();
        }
    }
}
