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

namespace Rx.Nyss.Web.Tests.Features.NationalSociety
{
    public class NationalSocietyServiceTests
    {
        private readonly INationalSocietyService _nationalSocietyService;

        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private const string NationalSocietyName = "Norway";
        private const int NationalSocietyId = 1;
        private const int CountryId = 1;
        private const int ContentLanguageId = 1;

        public NationalSocietyServiceTests()
        {
            _nyssContext = Substitute.For<INyssContext>();
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nationalSocietyService = new NationalSocietyService(_nyssContext, _loggerAdapter);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ReturnSuccess()
        {
            var nationalSocietyReq = new CreateNationalSocietyRequestDto()
            {
                Name = NationalSocietyName,
                ContentLanguageId = ContentLanguageId,
                CountryId = CountryId
            };

            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Creation.Success);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSavingFails_ReturnError()
        {
            var nationalSocietyReq = new CreateNationalSocietyRequestDto()
            {
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            _nyssContext.SaveChangesAsync().Throws(new Exception());

            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task CreateNationalSociety_WhenNameAlreadyExists_ReturnError()
        {
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

            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            var result = await _nationalSocietyService.CreateNationalSociety(nationalSocietyReq);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Creation.NameAlreadyExists);
        }

        [Fact]
        public async Task EditNationalSociety_WhenSuccessful_ReturnSuccess()
        {
            var nationalSocietyReq = new EditNationalSocietyRequestDto()
            {
                Id = NationalSocietyId,
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            var result = await _nationalSocietyService.EditNationalSociety(nationalSocietyReq);

            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Success);
        }

        [Fact]
        public async Task EditNationalSociety_WhenSavingFails_ReturnError()
        {
            var nationalSocietyReq = new EditNationalSocietyRequestDto()
            {
                Id = NationalSocietyId,
                Name = NationalSocietyName,
                CountryId = CountryId,
                ContentLanguageId = ContentLanguageId
            };

            _nyssContext.SaveChangesAsync().Throws(new Exception());

            var result = await _nationalSocietyService.EditNationalSociety(nationalSocietyReq);

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task RemoveNationalSociety_WhenSuccessful_ReturnSuccess()
        {
            var result = await _nationalSocietyService.RemoveNationalSociety(NationalSocietyId);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task RemoveNationalSociety_WhenRemovingFails_ReturnError()
        {
            _nyssContext.SaveChangesAsync().Throws(new Exception());

            var result = await _nationalSocietyService.RemoveNationalSociety(NationalSocietyId);

            result.IsSuccess.ShouldBeFalse();
        }
    }
}
