using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocieties.Commands;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Tests.Features.NationalSocieties.TestData;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties.Commands
{
    public class EditNationalSocietyCommandTests
    {
        private readonly INyssContext _mockNyssContext;

        private readonly IAuthorizationService _mockAuthorizationService;

        private readonly EditNationalSocietyCommand.Handler _handler;

        public EditNationalSocietyCommandTests()
        {
            _mockNyssContext = Substitute.For<INyssContext>();
            _mockAuthorizationService = Substitute.For<IAuthorizationService>();

            _mockAuthorizationService.GetCurrentUserName().Returns("yo");

            _handler = new EditNationalSocietyCommand.Handler(
                _mockAuthorizationService,
                _mockNyssContext);
        }

        [Fact]
        public async Task EditNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var testData = new NationalSocietyServiceTestData(_mockNyssContext, Substitute.For<ISmsGatewayService>());
            testData.BasicData.Data.GenerateData().AddToDbContext();

            var cmd = new EditNationalSocietyCommand(
                BasicNationalSocietyServiceTestData.NationalSocietyId,
                new EditNationalSocietyCommand.RequestBody
                {
                    Name = BasicNationalSocietyServiceTestData.NationalSocietyName,
                    CountryId = BasicNationalSocietyServiceTestData.CountryId,
                    ContentLanguageId = BasicNationalSocietyServiceTestData.ContentLanguageId
                });

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Success);
        }
    }
}
