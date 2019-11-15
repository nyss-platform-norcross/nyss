using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSociety;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Features.User;

namespace Rx.Nyss.Web.Tests.Features.NationalSociety
{
    public class NationalSocietyServiceTests
    {
        private readonly INationalSocietyService _nationalSocietyService;

        private readonly INyssContext _nyssContextMock;
        private readonly ILoggerAdapter _loggerAdapterMock;
        private readonly IConfig _configMock;
        private readonly IUserService _userServiceMock;
        private const string NationalSocietyName = "Norway";
        private const string ExistingNationalSocietyName = "Poland";
        private const int NationalSocietyId = 1;
        private const int CountryId = 1;
        private const int ContentLanguageId = 1;

        public NationalSocietyServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _configMock = Substitute.For<IConfig>();
            _userServiceMock = Substitute.For<IUserService>();
            _nationalSocietyService = new NationalSocietyService(_nyssContextMock, _loggerAdapterMock, _userServiceMock);

            // Arrange

            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety>
            {
                new RX.Nyss.Data.Models.NationalSociety { Name = ExistingNationalSocietyName }
            };
            var contentLanguages = new List<RX.Nyss.Data.Models.ContentLanguage>
            {
                new RX.Nyss.Data.Models.ContentLanguage { Id = ContentLanguageId }
            };
            var countries = new List<RX.Nyss.Data.Models.Country>
            {
                new RX.Nyss.Data.Models.Country { Id = CountryId }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            var contentLanguagesMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ContentLanguages.Returns(contentLanguagesMockDbSet);
            var countriesMockDbSet = countries.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Countries.Returns(countriesMockDbSet);

            _nyssContextMock.NationalSocieties.FindAsync(NationalSocietyId).Returns(nationalSocieties[0]);
            _nyssContextMock.ContentLanguages.FindAsync(ContentLanguageId).Returns(contentLanguages[0]);
            _nyssContextMock.Countries.FindAsync(CountryId).Returns(countries[0]);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto()
            {
                Name = NationalSocietyName,
                ContentLanguageId = ContentLanguageId,
                CountryId = CountryId
            };

            // Actual
            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<RX.Nyss.Data.Models.NationalSociety>());
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
        public async Task CreateNationalSociety_WhenSavingFails_ShouldReturnError()
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto()
            {
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            _nyssContextMock.SaveChangesAsync().Throws(new Exception());

            // Actual
            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenNameAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto()
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
            var nationalSocietyReq = new EditNationalSocietyRequestDto()
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
        public async Task EditNationalSociety_WhenSavingFails_ShouldReturnError()
        {
            // Arrange
            var nationalSocietyReq = new EditNationalSocietyRequestDto()
            {
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            _nyssContextMock.SaveChangesAsync().Throws(new Exception());

            // Actual
            var result = await _nationalSocietyService.EditNationalSociety(NationalSocietyId, nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
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
        public async Task RemoveNationalSociety_WhenRemovingFails_ShouldReturnError()
        {
            // Arrange
            _nyssContextMock.SaveChangesAsync().Throws(new Exception());

            // Actual
            var result = await _nationalSocietyService.RemoveNationalSociety(NationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }
    }
}
