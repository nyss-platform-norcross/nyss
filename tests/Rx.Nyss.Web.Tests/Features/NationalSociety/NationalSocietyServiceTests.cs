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
using Microsoft.EntityFrameworkCore;
using Rx.Nyss.Web.Tests.Common;

namespace Rx.Nyss.Web.Tests.Features.NationalSociety
{
    public class NationalSocietyServiceTests
    {
        private readonly INationalSocietyService _nationalSocietyService;

        private readonly INyssContext _nyssContextMock;
        private readonly ILoggerAdapter _loggerAdapterMock;
        private const string NationalSocietyName = "Norway";
        private const int NationalSocietyId = 1;
        private const int CountryId = 1;
        private const int ContentLanguageId = 1;

        public NationalSocietyServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _nationalSocietyService = new NationalSocietyService(_nyssContextMock, _loggerAdapterMock);

            // Arrange

            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety>();
            var contentLanguages = new List<RX.Nyss.Data.Models.ContentLanguage>()
            {
                new RX.Nyss.Data.Models.ContentLanguage() { Id = ContentLanguageId }
            };
            var countries = new List<RX.Nyss.Data.Models.Country>()
            {
                new RX.Nyss.Data.Models.Country() { Id = CountryId }
            };

            var contentLanguageDb = Substitute.For<DbSet<RX.Nyss.Data.Models.ContentLanguage>, IQueryable<RX.Nyss.Data.Models.ContentLanguage>>();
            DbSetInitializer.InitDb(contentLanguageDb, contentLanguages);
            
            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            //var contentLanguagesMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ContentLanguages.Returns(contentLanguageDb);
            var countriesMockDbSet = countries.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Countries.Returns(countriesMockDbSet);
            // _nyssContextMock.ContentLanguages.FindAsync(1).Returns(contentLanguages[0]);
            // _nyssContextMock.Countries.FindAsync(1).Returns(countries[0]);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ReturnSuccess()
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
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Creation.Success);
        }

        [Theory]
        [InlineData(CountryId, 2)]
        [InlineData(2, ContentLanguageId)]
        public async Task CreateNationalSociety_WhenRequiredParamsAreMissing_ShouldReturnError(int countryId, int contentLanguageId)
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
            result.Message.Key.ShouldBeOneOf(ResultKey.NationalSociety.Creation.CountryNotDefined, ResultKey.NationalSociety.Creation.LanguageNotDefined);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSavingFails_ReturnError()
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
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Creation.Error);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenNameAlreadyExists_ReturnError()
        {
            // Arrange
            var nationalSocietyReq = new CreateNationalSocietyRequestDto()
            {
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety>()
            {
                new RX.Nyss.Data.Models.NationalSociety() { Name = NationalSocietyName }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            // Actual
            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Creation.NameAlreadyExists);
        }

        [Fact]
        public async Task EditNationalSociety_WhenSuccessful_ReturnSuccess()
        {
            // Arrange
            var nationalSocietyReq = new EditNationalSocietyRequestDto()
            {
                Id = NationalSocietyId,
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            // Actual
            var result = await _nationalSocietyService.EditNationalSociety(nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Success);
        }

        [Fact]
        public async Task EditNationalSociety_WhenSavingFails_ReturnError()
        {
            // Arrange
            var nationalSocietyReq = new EditNationalSocietyRequestDto()
            {
                Id = NationalSocietyId,
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            _nyssContextMock.SaveChangesAsync().Throws(new Exception());

            // Actual
            var result = await _nationalSocietyService.EditNationalSociety(nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Error);
        }

        [Fact]
        public async Task RemoveNationalSociety_WhenSuccessful_ReturnSuccess()
        {
            // Actual
            var result = await _nationalSocietyService.RemoveNationalSociety(NationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Remove.Success);
        }

        [Fact]
        public async Task RemoveNationalSociety_WhenRemovingFails_ReturnError()
        {
            // Arrange
            _nyssContextMock.SaveChangesAsync().Throws(new Exception());

            // Actual
            var result = await _nationalSocietyService.RemoveNationalSociety(NationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Remove.Error);
        }
    }
}
