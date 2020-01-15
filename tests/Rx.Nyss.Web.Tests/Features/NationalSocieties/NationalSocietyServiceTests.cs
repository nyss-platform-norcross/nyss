using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety;
using RX.Nyss.Web.Features.NationalSociety.Access;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties
{
    public class NationalSocietyServiceTests
    {
        private readonly INationalSocietyService _nationalSocietyService;

        private readonly INyssContext _nyssContextMock;
        private const string NationalSocietyName = "Norway";
        private const string ExistingNationalSocietyName = "Poland";
        private const int NationalSocietyId = 1;
        private const int CountryId = 1;
        private const int ContentLanguageId = 1;
        private const int ConsentId = 1;

        public NationalSocietyServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            var authorizationService = Substitute.For<IAuthorizationService>();
            authorizationService.GetCurrentUserName().Returns("yo");

            _nationalSocietyService = new NationalSocietyService(_nyssContextMock, Substitute.For<INationalSocietyAccessService>(), loggerAdapterMock, authorizationService);

            // Arrange

            var users = new List<User> { new ManagerUser { EmailAddress = "yo" } };

            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety { Name = ExistingNationalSocietyName, PendingHeadManager = users[0] }
            };
            var contentLanguages = new List<ContentLanguage>
            {
                new ContentLanguage { Id = ContentLanguageId }
            };
            var countries = new List<Country>
            {
                new Country { Id = CountryId }
            };
            var headManagerConsents = new List<HeadManagerConsent> { new HeadManagerConsent
            {
                Id = ConsentId,
                NationalSocietyId = NationalSocietyId
            } };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            var contentLanguagesMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ContentLanguages.Returns(contentLanguagesMockDbSet);
            var countriesMockDbSet = countries.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Countries.Returns(countriesMockDbSet);
            var headManagerConsentsMockDbSet = headManagerConsents.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HeadManagerConsents.Returns(headManagerConsentsMockDbSet);
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(mockDbSet);

            _nyssContextMock.NationalSocieties.FindAsync(NationalSocietyId).Returns(nationalSocieties[0]);
            _nyssContextMock.ContentLanguages.FindAsync(ContentLanguageId).Returns(contentLanguages[0]);
            _nyssContextMock.Countries.FindAsync(CountryId).Returns(countries[0]);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto
            {
                Name = NationalSocietyName,
                ContentLanguageId = ContentLanguageId,
                CountryId = CountryId
            };

            // Actual
            await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<NationalSociety>());
        }

        [Theory]
        [InlineData(CountryId, 2)]
        [InlineData(2, ContentLanguageId)]
        public async Task CreateNationalSociety_WhenLanguageOrCountryNotFound_ShouldReturnError(int countryId, int contentLanguageId)
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto
            {
                Name = NationalSocietyName,
                CountryId = countryId,
                ContentLanguageId = contentLanguageId
            };

            // Actual
            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBeOneOf(ResultKey.NationalSociety.Creation.CountryNotFound, ResultKey.NationalSociety.Creation.LanguageNotFound);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenNameAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto
            {
                Name = ExistingNationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            // Actual
            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Creation.NameAlreadyExists);
        }

        [Fact]
        public async Task EditNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var nationalSocietyReq = new EditNationalSocietyRequestDto
            {
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            // Actual
            var result = await _nationalSocietyService.EditNationalSociety(NationalSocietyId, nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Success);
        }

        [Fact]
        public async Task RemoveNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Actual
            var result = await _nationalSocietyService.RemoveNationalSociety(NationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Remove.Success);
        }

        [Fact]
        public async Task SetAsHead_WhenOk_ShouldBeOk()
        {
            // Actual
            var result = await _nationalSocietyService.SetAsHeadManager();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.HeadManagerConsents.Received(1).AddAsync(Arg.Any<HeadManagerConsent>());
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SetAsHead_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var users = new List<User>{new ManagerUser{EmailAddress = "no-yo"}};
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(mockDbSet);

            // Actual
            var result = await _nationalSocietyService.SetAsHeadManager();

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }
    }
}
