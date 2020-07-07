using FluentValidation.TestHelper;
using NSubstitute;
using NUnit.Framework;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.DataCollectors.Validation;

namespace RX.Nyss.Web.Tests.Validators
{
    [TestFixture]
    public class DataCollectorValidationTests
    {
        private CreateDataCollectorRequestDto.Validator CreateValidator { get; set; }
        private EditDataCollectorRequestDto.Validator EditValidator { get; set; }

        public DataCollectorValidationTests()
        {
            var validationService = Substitute.For<IDataCollectorValidationService>();
            validationService.PhoneNumberExists("+4712345678").Returns(true);
            CreateValidator = new CreateDataCollectorRequestDto.Validator(validationService);
            EditValidator = new EditDataCollectorRequestDto.Validator(validationService);
        }

        [Test]
        public void Create_WhenNameExists_ShouldHaveError()
        {
            CreateValidator.ShouldHaveValidationErrorFor(dc => dc.PhoneNumber, "+4712345678");
        }

        [Test]
        public void Create_WhenNameDoesntExists_ShouldNotHaveError()
        {
            CreateValidator.ShouldNotHaveValidationErrorFor(dc => dc.PhoneNumber, "+4712345679");
        }
    }
}