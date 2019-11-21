using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.Logging;
using Xunit;

namespace Rx.Nyss.Web.Tests.Services
{
    public class EmailToSMSServiceTests
    {
        private readonly IEmailToSMSService _emailToSMSService;
        private readonly IEmailPublisherService _emailPublisherServiceMock;
        private readonly IConfig _configMock;

        public EmailToSMSServiceTests()
        {
            _emailPublisherServiceMock = Substitute.For<IEmailPublisherService>();
            _configMock = new NyssConfig() { EmailToSMSDomain = "domain.com" };
            _emailToSMSService = new EmailToSMSService(_emailPublisherServiceMock, _configMock);
        }

        [Fact]
        public async Task SendMessage_WhenSuccessful_ShouldCallEmailPublisherService()
        {
            // Arrange
            var apiKey = "someapikey";
            List<string> recipients = new List<string>
            {
                "+47123143513"
            };
            var message = "Thanks for your message";

            // Act
            await _emailToSMSService.SendMessage(apiKey, recipients, message);

            await _emailPublisherServiceMock.Received(1).SendEmail(Arg.Any<(string, string)>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
