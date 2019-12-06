using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Xunit;

namespace RX.Nyss.FuncApp.Tests
{
    public class EmailServiceTests
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailService> _loggerMock;
        private readonly IConfig _configurationMock;
        private readonly IMailjetEmailClient _mailjetEmailClientMock;

        public EmailServiceTests()
        {
            _loggerMock = Substitute.For<ILogger<EmailService>>();
            _configurationMock = Substitute.For<IConfig>();
            _configurationMock.MailjetConfig = new NyssFuncAppConfig.MailjetConfigOptions();
            _mailjetEmailClientMock = Substitute.For<IMailjetEmailClient>();
            _emailService = new EmailService(
                _loggerMock,
                _configurationMock,
                _mailjetEmailClientMock);
        }

        [Theory]
        [InlineData("user@example.com")]
        public async Task SendEmailWithMailjet_WhenSendToAllFlagIsMissing_ShouldUseSandboxModeAndLogWarning(string email)
        {
            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }}, "hey@example.com", "");

            // Assert
            await _mailjetEmailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), Arg.Is(true));
        }

        [Theory]
        [InlineData("user@example.com")]
        public async Task SendEmailWithMailjet_WhenSendToAllIsFalse_ShouldUseSandboxMode(string email)
        {
            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }}, "", "");

            // Assert
            await _mailjetEmailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), Arg.Is(true));
        }

        [Theory]
        [InlineData("user@example.com", false)]
        [InlineData("donald.duck@example.com", false)]
        [InlineData("scrooge.mc.duck@example.com", true)]
        public async Task SendEmailWithMailjet_WhenSendToAllIsFalse_ShouldUseSandboxModeUnlessWhiteListed(string email, bool sandboxMode)
        {
            // Arrange
            var whitelist = @"
            user@example.com
            donald.duck@example.com
            some@email.no";

            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }}, whitelist, "");

            // Assert
            await _mailjetEmailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), Arg.Is(sandboxMode));
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("donald.duck@example.com")]
        [InlineData("scrooge.mc.duck@example.com")]
        public async Task SendEmailWithMailjet_WhenSendToAllIsTrue_ShouldSendToAll(string email)
        {
            // Arrange
            _configurationMock.MailjetConfig.SendToAll = true;
            var whitelist = @"
            user@example.com
            donald.duck@example.com
            some@email.no";

            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }}, whitelist, "");

            // Assert
            await _mailjetEmailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), false);
        }

        [Theory]
        [InlineData("user@example.com", "+4712345678")]
        public async Task SendEmailWithMailjet_WhenMessageSendAsTextOnlyIsTrue_ShouldSendAsTextOnly(string email, string phoneNumber)
        {
            // Arrange
            var whitelist = "user@example.com";
            var phoneNumberWhitelist = "+4712345678";

            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }, Subject = phoneNumber, SendAsTextOnly = true}, whitelist, phoneNumberWhitelist);

            // Assert
            await _mailjetEmailClientMock.Received(1).SendEmailAsTextOnly(Arg.Any<SendEmailMessage>(), Arg.Is(false));
        }

        [Theory]
        [InlineData("user@example.com", "+4712345679")]
        public async Task SendEmailWithMailjet_WhenPhoneNumberIsNotWhitelisted_ShouldNotSendEmail(string email, string phoneNumber)
        {
            // Arrange
            _configurationMock.MailjetConfig.SendFeedbackSmsToAll = false;
            var whitelist = @"
            user@example.com
            donald.duck@example.com
            some@email.no";
            var phoneNumberWhitelist = "+4712345678";

            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }, Subject = phoneNumber, SendAsTextOnly = true}, whitelist, phoneNumberWhitelist);

            // Assert
            await _mailjetEmailClientMock.DidNotReceive().SendEmailAsTextOnly(Arg.Any<SendEmailMessage>(), Arg.Is(false));
        }

        [Theory]
        [InlineData("user@example.com", "+4712345678")]
        public async Task SendEmailWithMailjet_WhenSendFeedbackSmsToAllIsTrue_ShouldSendToAll(string email, string phoneNumber)
        {
            // Arrange
            _configurationMock.MailjetConfig.SendFeedbackSmsToAll = true;
            var whitelist = "user@example.com";
            var phoneNumberWhitelist = "+4712345678";

            // Act
            await _emailService.SendEmailWithMailjet(new SendEmailMessage{To = new Contact{ Email = email }, Subject = phoneNumber, SendAsTextOnly = true}, whitelist, phoneNumberWhitelist);

            // Assert
            await _mailjetEmailClientMock.Received(1).SendEmailAsTextOnly(Arg.Any<SendEmailMessage>(), Arg.Is(false));
        }
    }
}
