using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils.Logging;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Services
{
    public class StringsResourcesServiceTests
    {
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

        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IGeneralBlobProvider _generalBlobProvider;

        public StringsResourcesServiceTests()
        {
            var logger = Substitute.For<ILoggerAdapter>();
            _generalBlobProvider = Substitute.For<IGeneralBlobProvider>();

            _stringsResourcesService = new StringsResourcesService(_generalBlobProvider, logger);
        }

        [Theory]
        [InlineData("login.signIn", "en", "Log in")]
        [InlineData("login.signIn", "fr", "S'identifier")]
        [InlineData("login.signIn", "wrong-lang", "login.signIn")]
        public async Task GetStringsResources_ReturnsProperTranslations(string key, string languageCode, string value)
        {
            _generalBlobProvider.GetStringsResources().Returns(BlobValue);

            var result = await _stringsResourcesService.GetStrings(languageCode);

            result[key].ShouldBe(value);
        }
    }
}
