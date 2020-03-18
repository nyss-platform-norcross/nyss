using Microsoft.Extensions.Logging;
using NSubstitute;
using RX.Nyss.FuncApp.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.FuncApp.Tests
{
    public class WhitelistValidatorTests
    {
        private readonly IWhitelistValidator _whitelistValidator;

        public WhitelistValidatorTests()
        {
            var loggerMock = Substitute.For<ILogger<WhitelistValidator>>();
            _whitelistValidator = new WhitelistValidator(loggerMock);
        }

        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("donald.duck@example.com", true)]
        [InlineData("scrooge.mc.duck@example.com", false)]
        public void IsWhitelistedEmailAddress_ShouldOnlyReturnTrueForTheOnesOnTheList(string email, bool whitelisted)
        {
            // Arrange
            var whitelist = @"
            user@example.com
            donald.duck@example.com
            some@email.no";

            // Act
            var result = _whitelistValidator.IsWhitelistedEmailAddress(whitelist, email);

            // Assert
            result.ShouldBe(whitelisted);
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
    }
}
