using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.NationalSocieties;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.TechnicalAdvisors;
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

        private readonly IManagerService _managerServiceMock;
        private readonly ITechnicalAdvisorService _technicalAdvisorServiceMock;
        private readonly IAuthorizationService _authorizationServiceMock;

        public NationalSocietyServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _authorizationServiceMock = Substitute.For<IAuthorizationService>();
            _authorizationServiceMock.GetCurrentUserName().Returns("yo");
            _managerServiceMock = Substitute.For<IManagerService>();
            _technicalAdvisorServiceMock = Substitute.For<ITechnicalAdvisorService>();
            _smsGatewayServiceMock = Substitute.For<ISmsGatewayService>();

            _nationalSocietyService = new NationalSocietyService(
                _nyssContextMock,
                Substitute.For<INationalSocietyAccessService>(),
                loggerAdapterMock,
                _authorizationServiceMock);

            _testData = new NationalSocietyServiceTestData(_nyssContextMock, _smsGatewayServiceMock);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var nationalSocietyReq = new CreateNationalSocietyRequestDto
            {
                Name = BasicNationalSocietyServiceTestData.NationalSocietyName,
                ContentLanguageId = BasicNationalSocietyServiceTestData.ContentLanguageId,
                CountryId = BasicNationalSocietyServiceTestData.CountryId
            };

            // Act
            await _nationalSocietyService.Create(nationalSocietyReq);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<NationalSociety>());
        }

        [Fact]
        public async Task EditNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var nationalSocietyReq = new EditNationalSocietyRequestDto
            {
                Name = BasicNationalSocietyServiceTestData.NationalSocietyName,
                CountryId = BasicNationalSocietyServiceTestData.CountryId,
                ContentLanguageId = BasicNationalSocietyServiceTestData.ContentLanguageId
            };

            // Act
            var result = await _nationalSocietyService.Edit(BasicNationalSocietyServiceTestData.NationalSocietyId, nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Success);
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
