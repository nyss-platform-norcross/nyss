using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.DataCollectors.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataCollectors
{
    public class DataCollectorValidationTests
    {
        private CreateDataCollectorRequestDto.Validator CreateValidator { get; }
        private EditDataCollectorRequestDto.Validator EditValidator { get; }

        public DataCollectorValidationTests()
        {
            var validationService = Substitute.For<IDataCollectorValidationService>();
            validationService.PhoneNumberExists("+4712345678").Returns(true);
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
    }
}
