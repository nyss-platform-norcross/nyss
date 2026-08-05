using System.Linq;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RX.Nyss.FuncApp.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.FuncApp.Tests
{
    public class WhitelistValidatorTests
    {
        private readonly ILogger<WhitelistValidator> _loggerMock;
        private readonly IWhitelistValidator _whitelistValidator;

        public WhitelistValidatorTests()
        {
            _loggerMock = Substitute.For<ILogger<WhitelistValidator>>();
            _whitelistValidator = new WhitelistValidator(_loggerMock);
        }

        private void ShouldHaveLogged(LogLevel level, string messageContains)
        {
            _loggerMock.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
                .Select(call => call.GetArguments())
                .Any(args =>
                    args.Length >= 3 &&
                    args[0] is LogLevel loggedLevel &&
                    loggedLevel == level &&
                    args[2] != null &&
                    args[2].ToString().Contains(messageContains))
                .ShouldBeTrue($"Expected a {level} log containing '{messageContains}'.");
        }

        [Theory]
        [InlineData("user@test.com", true)]
        [InlineData("donald.duck@test.com", true)]
        [InlineData("scrooge.mc.duck@test.com", false)]
        public void IsWhitelistedEmailAddress_ShouldOnlyReturnTrueForTheOnesOnTheList(string email, bool whitelisted)
        {
            // Arrange
            var whitelist = @"
            user@test.com
            donald.duck@test.com
            some@email.no";

            // Act
            var result = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, email);

            // Assert
            result.ShouldBe(whitelisted);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsWhitelistedEmailAddress_WhenWhitelistIsEmptyOrWhitespace_ShouldReturnFalse(string whitelist)
        {
            // Act
            var result = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, "user@test.com");

            // Assert
            result.ShouldBeFalse();
            ShouldHaveLogged(LogLevel.Critical, "The email whitelist is empty.");
        }

        [Fact]
        public void IsWhitelistedEmailAddress_WhenEntriesHaveWhitespace_ShouldTrimAndMatch()
        {
            // Arrange
            var whitelist = "  user@test.com  \n\tdonald.duck@test.com\t";

            // Act
            var trimmedMatch = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, "user@test.com");
            var otherMatch = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, "donald.duck@test.com");
            var noMatch = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, "other@test.com");

            // Assert
            trimmedMatch.ShouldBeTrue();
            otherMatch.ShouldBeTrue();
            noMatch.ShouldBeFalse();
        }

        [Theory]
        [InlineData("user@test.com\ndonald.duck@test.com", "user@test.com", true)]
        [InlineData("user@test.com\r\ndonald.duck@test.com", "donald.duck@test.com", true)]
        [InlineData("user@test.com\rdonald.duck@test.com", "donald.duck@test.com", true)]
        [InlineData("user@test.com\r\ndonald.duck@test.com", "missing@test.com", false)]
        public void IsWhitelistedEmailAddress_ShouldHandleDifferentLineEndings(string whitelist, string email, bool expected)
        {
            // Act
            var result = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, email);

            // Assert
            result.ShouldBe(expected);
        }

        [Theory]
        [InlineData("+47123456", true)]
        [InlineData("+123456", true)]
        [InlineData("+45123456", true)]
        [InlineData("+555555", false)]
        public void IsWhiteListedPhoneNumber_ShouldOnlyReturnTrueForTheOnesOnTheList(string phoneNumber, bool whitelisted)
        {
            // Arrange
            var whitelist = @"
            +47123456
            +123456
            +45123456";

            // Act
            var result = _whitelistValidator.IsWhiteListedPhoneNumber(whitelist, phoneNumber);

            // Assert
            result.ShouldBe(whitelisted);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsWhiteListedPhoneNumber_WhenWhitelistIsEmptyOrWhitespace_ShouldReturnFalse(string whitelist)
        {
            // Act
            var result = _whitelistValidator.IsWhiteListedPhoneNumber(whitelist, "+47123456");

            // Assert
            result.ShouldBeFalse();
            ShouldHaveLogged(LogLevel.Critical, "The sms whitelist is empty.");
        }

        [Fact]
        public void IsWhiteListedPhoneNumber_WhenEntriesHaveWhitespace_ShouldTrimAndMatch()
        {
            // Arrange
            var whitelist = "  +47123456  \n\t+123456\t";

            // Act
            var trimmedMatch = _whitelistValidator.IsWhiteListedPhoneNumber(whitelist, "+47123456");
            var otherMatch = _whitelistValidator.IsWhiteListedPhoneNumber(whitelist, "+123456");
            var noMatch = _whitelistValidator.IsWhiteListedPhoneNumber(whitelist, "+999999");

            // Assert
            trimmedMatch.ShouldBeTrue();
            otherMatch.ShouldBeTrue();
            noMatch.ShouldBeFalse();
        }

        [Theory]
        [InlineData("+47123456\n+123456", "+47123456", true)]
        [InlineData("+47123456\r\n+123456", "+123456", true)]
        [InlineData("+47123456\r+123456", "+123456", true)]
        [InlineData("+47123456\r\n+123456", "+999999", false)]
        public void IsWhiteListedPhoneNumber_ShouldHandleDifferentLineEndings(string whitelist, string phoneNumber, bool expected)
        {
            // Act
            var result = _whitelistValidator.IsWhiteListedPhoneNumber(whitelist, phoneNumber);

            // Assert
            result.ShouldBe(expected);
        }
    }
}
