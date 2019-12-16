using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;
using Xunit;

namespace RX.Nyss.Web.Tests.Services
{
    public class EmailToSMSServiceTests
    {
        private readonly IEmailToSMSService _emailToSMSService;
        private readonly IEmailPublisherService _emailPublisherServiceMock;

        public EmailToSMSServiceTests()
        {
            _emailPublisherServiceMock = Substitute.For<IEmailPublisherService>();
            _emailToSMSService = new EmailToSMSService(_emailPublisherServiceMock);
        }

        [Fact]
        public async Task SendMessage_WhenSuccessful_ShouldCallEmailPublisherService()
        {
            // Arrange
            var recipients = new List<string>
            {
                "+47123143513"
            };
            var message = "Thanks for your message";

            var gatewaySetting = new GatewaySetting
            {
                Id = 1,
                EmailAddress = "test@domain.com"
            };

            // Act
            await _emailToSMSService.SendMessage(gatewaySetting, recipients, message);

            await _emailPublisherServiceMock.Received(1).SendEmail(Arg.Any<(string, string)>(), Arg.Is<string>(_ => _ == "+47123143513"), Arg.Is<string>(body => body == "Thanks for your message"), Arg.Is<bool>(_ => _));
        }
    }
}
