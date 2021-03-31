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
            CreateValidator.ShouldHaveValidationErrorFor(dc => dc.PhoneNumber, "+4712345678");
        }

        [Fact]
        public void Create_WhenNameDoesntExists_ShouldNotHaveError()
        {
            CreateValidator.ShouldNotHaveValidationErrorFor(dc => dc.PhoneNumber, "+4712345679");
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
            var result = EditValidator.TestValidate(new EditDataCollectorRequestDto
            {
                Id = 1,
                PhoneNumber = "+4712345678"
            });

            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
        }

        [Fact]
        public void Edit_WhenNameDoesntExists_ShouldNotHaveError()
        {
            var result = EditValidator.TestValidate(new EditDataCollectorRequestDto
            {
                Id = 2,
                PhoneNumber = "+4712345678"
            });

            result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
        }
    }
}