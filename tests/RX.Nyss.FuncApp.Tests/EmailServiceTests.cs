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
        private readonly IEmailClient _emailClientMock;
        private readonly IWhitelistValidator _whitelistValidator;

        public EmailServiceTests()
        {
            _loggerMock = Substitute.For<ILogger<EmailService>>();
            _configurationMock = Substitute.For<IConfig>();
            _configurationMock.MailConfig = new NyssFuncAppConfig.MailConfigOptions() { EnableFeedbackSms = true };
            _emailClientMock = Substitute.For<IEmailClient>();
            _whitelistValidator = Substitute.For<IWhitelistValidator>();
            _emailService = new EmailService(
                _loggerMock,
                _configurationMock,
                _emailClientMock,
                _whitelistValidator);
        }

        [Theory]
        [InlineData("user@example.com")]
        public async Task SendEmailWithMailjet_WhenSendToAllFlagIsMissing_ShouldUseSandboxModeAndLogWarning(string email)
        {
            // Act
            await _emailService.SendEmail(new SendEmailMessage { To = new Contact { Email = email } }, "hey@example.com", "");

            // Assert
            await _emailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), Arg.Is(true));
        }

        [Theory]
        [InlineData("user@example.com")]
        public async Task SendEmailWithMailjet_WhenSendToAllIsFalse_ShouldUseSandboxMode(string email)
        {
            // Act
            await _emailService.SendEmail(new SendEmailMessage { To = new Contact { Email = email } }, "", "");

            // Assert
            await _emailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), Arg.Is(true));
        }

        [Fact]
        public async Task SendEmailWithMailjet_WhenSendToAllIsFalse_ShouldUseSandboxModeUnlessWhiteListed()
        {
            // Arrange
            var whitelistedEmail = "whitelisted@email.com";
            var notWhitelistedEmail = "not_whitelisted@email.com";
            _whitelistValidator.IsWhitelistedEmailAddress(Arg.Any<string>(), whitelistedEmail).Returns(true);
            _whitelistValidator.IsWhitelistedEmailAddress(Arg.Any<string>(), notWhitelistedEmail).Returns(false);

            // Act
            await _emailService.SendEmail(new SendEmailMessage { To = new Contact { Email = whitelistedEmail } }, whitelistedEmail, "");
            await _emailService.SendEmail(new SendEmailMessage { To = new Contact { Email = notWhitelistedEmail } }, whitelistedEmail, "");

            // Assert
            await _emailClientMock.Received(1).SendEmail(Arg.Is<SendEmailMessage>(x => x.To.Email == notWhitelistedEmail), true);
            await _emailClientMock.Received(1).SendEmail(Arg.Is<SendEmailMessage>(x => x.To.Email == whitelistedEmail), false);
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("donald.duck@example.com")]
        [InlineData("scrooge.mc.duck@example.com")]
        public async Task SendEmailWithMailjet_WhenSendToAllIsTrue_ShouldSendToAll(string email)
        {
            // Arrange
            _configurationMock.MailConfig.SendToAll = true;
            var whitelist = @"
            user@example.com
            donald.duck@example.com
            some@email.no";

            // Act
            await _emailService.SendEmail(new SendEmailMessage { To = new Contact { Email = email } }, whitelist, "");

            // Assert
            await _emailClientMock.Received(1).SendEmail(Arg.Any<SendEmailMessage>(), false);
        }

        [Theory]
        [InlineData("user@example.com", "+4712345678")]
        public async Task SendEmailWithMailjet_WhenMessageSendAsTextOnlyIsTrue_ShouldSendAsTextOnly(string email, string phoneNumber)
        {
            // Arrange
            var whitelist = "user@example.com";
            var phoneNumberWhitelist = "+4712345678";

            _whitelistValidator.IsWhitelistedEmailAddress(whitelist, email).Returns(true);
            _whitelistValidator.IsWhiteListedPhoneNumber(phoneNumberWhitelist, phoneNumber).Returns(true);

            // Act
            await _emailService.SendEmail(new SendEmailMessage
            {
                To = new Contact { Email = email },
                Subject = phoneNumber,
                SendAsTextOnly = true
            }, whitelist, phoneNumberWhitelist);

            // Assert
            await _emailClientMock.Received(1).SendEmailAsTextOnly(Arg.Any<SendEmailMessage>(), Arg.Is(false));
        }

        [Theory]
        [InlineData("user@example.com", "+4712345679")]
        public async Task SendEmailWithMailjet_WhenPhoneNumberIsNotWhitelisted_ShouldNotSendEmail(string email, string phoneNumber)
        {
            // Arrange
            _configurationMock.MailConfig.SendFeedbackSmsToAll = false;
            var whitelist = @"
            user@example.com
            donald.duck@example.com
            some@email.no";
            var phoneNumberWhitelist = "+4712345678";

            // Act
            await _emailService.SendEmail(new SendEmailMessage
            {
                To = new Contact { Email = email },
                Subject = phoneNumber,
                SendAsTextOnly = true
            }, whitelist, phoneNumberWhitelist);

            // Assert
            await _emailClientMock.DidNotReceive().SendEmailAsTextOnly(Arg.Any<SendEmailMessage>(), Arg.Is(false));
        }

        [Theory]
        [InlineData("user@example.com", "+4712345678")]
        public async Task SendEmailWithMailjet_WhenSendFeedbackSmsToAllIsTrue_ShouldSendToAll(string email, string phoneNumber)
        {
            // Arrange
            _configurationMock.MailConfig.SendFeedbackSmsToAll = true;
            var whitelist = "user@example.com";
            var phoneNumberWhitelist = "+4712345678";

            // Act
            await _emailService.SendEmail(new SendEmailMessage
            {
                To = new Contact { Email = email },
                Subject = phoneNumber,
                SendAsTextOnly = true
            }, whitelist, phoneNumberWhitelist);

            // Assert
            await _emailClientMock.Received(1).SendEmailAsTextOnly(Arg.Any<SendEmailMessage>(), Arg.Is(false));
        }
    }
}
