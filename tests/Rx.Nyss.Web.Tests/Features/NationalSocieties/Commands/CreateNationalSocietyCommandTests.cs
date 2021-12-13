using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSocieties.Commands;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Tests.Features.NationalSocieties.TestData;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties.Commands
{
    public class CreateNationalSocietyCommandTests
    {
        private readonly INyssContext _mockNyssContext;

        private readonly CreateNationalSocietyCommand.Handler _handler;

        public CreateNationalSocietyCommandTests()
        {
            _mockNyssContext = Substitute.For<INyssContext>();

            _handler = new CreateNationalSocietyCommand.Handler(
                _mockNyssContext,
                Substitute.For<ILoggerAdapter>());
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            const string name = "Obi-Wan Kenobi";

            var testData = new NationalSocietyServiceTestData(_mockNyssContext, Substitute.For<ISmsGatewayService>());
            testData.BasicData.Data.GenerateData().AddToDbContext();

            var cmd = new CreateNationalSocietyCommand
            {
                Name = name,
                ContentLanguageId = BasicNationalSocietyServiceTestData.ContentLanguageId,
                CountryId = BasicNationalSocietyServiceTestData.CountryId
            };

            // Act
            await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await _mockNyssContext.Received(1).AddAsync(
                Arg.Is<NationalSociety>(ns => ns.Name == name));
        }
    }
}
