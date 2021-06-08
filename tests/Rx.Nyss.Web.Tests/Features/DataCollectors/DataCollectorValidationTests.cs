using System.Collections.Generic;
using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.DataCollectors.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors
{

    public class DataCollectorValidationTests
    {
        private CreateDataCollectorRequestDto.Validator CreateValidator { get; set; }
        private EditDataCollectorRequestDto.Validator EditValidator { get; set; }

        public DataCollectorValidationTests()
        {
            var validationService = Substitute.For<IDataCollectorValidationService>();
            validationService.PhoneNumberExists("+4712345678").Returns(true);
            validationService.PhoneNumberExistsToOther(1, "+4712345678").Returns(true);
            validationService.PhoneNumberExistsToOther(2, "+4712345678").Returns(false);
            CreateValidator = new CreateDataCollectorRequestDto.Validator(validationService);
            EditValidator = new EditDataCollectorRequestDto.Validator(validationService);
        }

        [Fact]
        public void Create_WhenNameExists_ShouldHaveError()
        {
            var dcRequestDto = new CreateDataCollectorRequestDto
            {
                PhoneNumber = "+4712345678",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = CreateValidator.TestValidate(dcRequestDto);

            result.ShouldHaveValidationErrorFor(dc => dc.PhoneNumber);
        }

        [Fact]
        public void Create_WhenNameDoesntExists_ShouldNotHaveError()
        {
            var dcRequestDto = new CreateDataCollectorRequestDto
            {
                PhoneNumber = "+4712345679",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = CreateValidator.TestValidate(dcRequestDto);

            result.ShouldNotHaveValidationErrorFor(dc => dc.PhoneNumber);
        }

        [Fact]
        public void Create_WhenCoordinatesAreOutOfBounds_ShouldHaveError()
        {
            CreateValidator.ShouldHaveValidationErrorFor(dc => dc.Locations, new List<DataCollectorLocationRequestDto>
            {
                new DataCollectorLocationRequestDto
                {
                    Latitude = 100,
                    Longitude = -200
                }
            });
        }

        [Fact]
        public void Create_WhenLocationListEmpty_ShouldHaveError()
        {
            var dcRequestDto = new CreateDataCollectorRequestDto
            {
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = CreateValidator.TestValidate(dcRequestDto);

            result.ShouldHaveValidationErrorFor(dc => dc.Locations);
        }

        [Fact]
        public void Edit_WhenCoordinatesAreOutOfBounds_ShouldHaveError()
        {
            EditValidator.ShouldHaveValidationErrorFor(dc => dc.Locations, new List<DataCollectorLocationRequestDto>
            {
                new DataCollectorLocationRequestDto
                {
                    Latitude = 100,
                    Longitude = -200
                }
            });
        }

        [Fact]
        public void Edit_WhenNameExistsForOtherDataCollector_ShouldHaveError()
        {
            var dcRequestDto = new EditDataCollectorRequestDto
            {
                Id = 1,
                PhoneNumber = "+4712345678",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = EditValidator.TestValidate(dcRequestDto);

            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        }

        [Fact]
        public void Edit_WhenNameDoesntExists_ShouldNotHaveError()
        {
            var dcRequestDto = new EditDataCollectorRequestDto
            {
                Id = 2,
                PhoneNumber = "+4712345678",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = EditValidator.TestValidate(dcRequestDto);

            result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        }

        [Fact]
        public void Edit_WhenLocationListEmpty_ShouldHaveError()
        {
            var dcRequestDto = new EditDataCollectorRequestDto
            {
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = EditValidator.TestValidate(dcRequestDto);

            result.ShouldHaveValidationErrorFor(dc => dc.Locations);
        }

    }
}