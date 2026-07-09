using System.Collections.Generic;
using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.DataCollectors.Commands;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.DataCollectors.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors
{

    public class DataCollectorValidationTests
    {
        private CreateDataCollectorCommandValidator CreateValidator { get; set; }
        private EditDataCollectorCommandValidator EditValidator { get; set; }
        private DataCollectorLocationRequestDto.Validator LocationValidator { get; set; }

        public DataCollectorValidationTests()
        {
            var validationService = Substitute.For<IDataCollectorValidationService>();
            validationService.PhoneNumberExists("+4712345678").Returns(true);
            validationService.PhoneNumberExistsToOther(1, "+4712345678").Returns(true);
            validationService.PhoneNumberExistsToOther(2, "+4712345678").Returns(false);
            CreateValidator = new CreateDataCollectorCommandValidator(validationService);
            EditValidator = new EditDataCollectorCommandValidator(validationService);
            LocationValidator = new DataCollectorLocationRequestDto.Validator(1, validationService);
        }

        [Fact]
        public void Create_WhenNameExists_ShouldHaveError()
        {
            var command = new CreateDataCollectorCommand
            {
                PhoneNumber = "+4712345678",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = CreateValidator.TestValidateAsync(command);
        }

        [Fact]
        public void Create_WhenNameDoesntExists_ShouldNotHaveError()
        {
            var command = new CreateDataCollectorCommand
            {
                PhoneNumber = "+4712345679",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = CreateValidator.TestValidateAsync(command);
        }

        [Fact]
        public void Create_WhenCoordinatesAreOutOfBounds_ShouldHaveError()
        {
            var command = new CreateDataCollectorCommand
            {
                Locations = new List<DataCollectorLocationRequestDto>
                {
                    new ()
                    {
                        Latitude = 100,
                        Longitude = -200
                    }
                }
            };

            var result = CreateValidator.TestValidateAsync(command);
        }

        [Fact]
        public void Create_WhenLocationListEmpty_ShouldHaveError()
        {
            var command = new CreateDataCollectorCommand
            {
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = CreateValidator.TestValidateAsync(command);
        }

        [Fact]
        public void Location_WhenCoordinatesAreOutOfBounds_ShouldHaveError()
        {
            var result = LocationValidator.TestValidateAsync(new DataCollectorLocationRequestDto
            {
                Latitude = 100,
                Longitude = -200
            });
        }

        [Fact]
        public void Location_WhenDuplicateVillage_ShouldHaveError()
        {
            // Arrange
            var dto = new DataCollectorLocationRequestDto { VillageId = 1 };
            var validationService = Substitute.For<IDataCollectorValidationService>();
            validationService.LocationHasDuplicateVillage(1, dto).Returns(true);
            LocationValidator = new DataCollectorLocationRequestDto.Validator(1, validationService);

            // Act
            var result = LocationValidator.TestValidateAsync(dto);
        }

        [Fact]
        public void Location_WhenDuplicateZone_ShouldHaveError()
        {
            // Arrange
            var dto = new DataCollectorLocationRequestDto { ZoneId = 1 };
            var validationService = Substitute.For<IDataCollectorValidationService>();
            validationService.LocationHasDuplicateZone(1, dto).Returns(true);
            LocationValidator = new DataCollectorLocationRequestDto.Validator(1, validationService);

            // Act
            var result = LocationValidator.TestValidateAsync(dto);
        }

        [Fact]
        public void Edit_WhenNameExistsForOtherDataCollector_ShouldHaveError()
        {
            var command = new EditDataCollectorCommand
            {
                Id = 1,
                PhoneNumber = "+4712345678",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = EditValidator.TestValidateAsync(command);
        }

        [Fact]
        public void Edit_WhenNameDoesntExists_ShouldNotHaveError()
        {
            var command = new EditDataCollectorCommand
            {
                Id = 2,
                PhoneNumber = "+4712345678",
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = EditValidator.TestValidateAsync(command);
        }

        [Fact]
        public void Edit_WhenLocationListEmpty_ShouldHaveError()
        {
            var command = new EditDataCollectorCommand
            {
                Locations = new List<DataCollectorLocationRequestDto>()
            };
            var result = EditValidator.TestValidateAsync(command);
        }

    }
}
