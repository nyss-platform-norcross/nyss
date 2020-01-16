using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils.Logging;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Services
{
    public class StringsResourcesServiceTests
    {
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly INyssBlobProvider _nyssBlobProvider;
        private const string DefaultLanguageCode = "en";

        public StringsResourcesServiceTests()
        {
            var logger = Substitute.For<ILoggerAdapter>();
            _nyssBlobProvider = Substitute.For<INyssBlobProvider>();

            _stringsResourcesService = new StringsResourcesService(_nyssBlobProvider, logger);
        }

        [Fact]
        public async Task GetStringsResources_WhenThrowsException_ShouldReturnError()
        {
            _nyssBlobProvider.GetStringsResources().Throws(new InvalidOperationException());

            var result = await _stringsResourcesService.GetStringsResources(DefaultLanguageCode);

            result.IsSuccess.ShouldBeFalse();
        }

        [Theory]
        [InlineData("login.signIn", "en", "Log in")]
        [InlineData("login.signIn", "fr", "S'identifier")]
        [InlineData("login.signIn", "wrong-lang", "login.signIn")]
        public async Task GetStringsResources_ReturnsProperTranslations(string key, string languageCode, string value)
        {
            _nyssBlobProvider.GetStringsResources().Returns(BlobValue);

            var result = await _stringsResourcesService.GetStringsResources(languageCode);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldContainKey(key);
            result.Value[key].ShouldBe(value);
        }

        private const string BlobValue = @"
            {
                ""strings"": [
                    {
                        ""key"": ""login.signIn"",
                        ""translations"": {
                            ""en"": ""Log in"",
                            ""fr"": ""S'identifier""
                        }
                    }
                ]
            }
        ";
    }
}
